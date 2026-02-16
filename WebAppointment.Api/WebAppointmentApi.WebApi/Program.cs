using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using WebAppointmentApi.Application;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Infrastructure;
using WebAppointmentApi.Infrastructure.Data;
using WebAppointmentApi.WebApi.Middleware;
using WebAppointmentApi.WebApi.Security;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');
    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
    var sslMode = query["sslmode"] ?? "Prefer";
    var connStr = $"Host={host};Port={port};Database={database};Username={userInfo[0]};Password={userInfo.ElementAtOrDefault(1) ?? ""};SSL Mode={sslMode};Trust Server Certificate=true";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;
}

builder.WebHost.UseUrls("http://localhost:5233");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum'ları string olarak parse/serialize et (ör. "Public" / "Private")
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Tüm 400 (model binding / FluentValidation) hatalarını sadece { message } formatında dön.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var first = context.ModelState
            .SelectMany(kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>())
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

        var msg = first ?? "Geçersiz giriş. Lütfen bilgileri kontrol edin.";
        return new ContentResult
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentType = "text/plain; charset=utf-8",
            Content = msg
        };
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAppointmentApi", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IClientInfoProvider, HttpContextClientInfoProvider>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();

var signingKey = builder.Configuration["Jwt:SigningKey"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorProfile", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new DoctorProfileRequirement());
    });
    options.AddPolicy("CanManageDepartment", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new WebAppointmentApi.WebApi.Security.Authorization.CanManageDepartmentRequirement());
    });
});

builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, DoctorProfileHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, WebAppointmentApi.WebApi.Security.Authorization.CanManageDepartmentHandler>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Çok fazla istek. Lütfen daha sonra tekrar deneyin.", token);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var key = context.User?.Identity?.IsAuthenticated == true
            ? (context.User.FindFirst("sub")?.Value ?? "auth")
            : (context.Connection.RemoteIpAddress?.ToString() ?? "anon");

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.AddPolicy("login", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "anon";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

// Dev/Local kolaylığı: migration uygula + seed çalıştır.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    // Not: DB seed işlemi kaldırıldı. Veritabanı tamamen boş kalmalıdır.
    // await DbSeeder.SeedAsync(app.Services);
}

// Dev-only: if admin login keeps failing because DB already had a different hash,
// allow a lightweight self-heal right before auth endpoints are executed.
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/login")
            || context.Request.Path.StartsWithSegments("/api/auth/register"))
        {
            // Seed/self-heal kaldırıldı.
            // using var scope = context.RequestServices.CreateScope();
            // await DbSeeder.SeedAsync(scope.ServiceProvider, context.RequestAborted);
        }

        await next();
    });
}

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAppointmentApi v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
