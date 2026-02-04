using System.Net;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Exceptions;

namespace WebAppointmentApi.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var status = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                ForbiddenException => (int)HttpStatusCode.Forbidden,
                NotFoundException => (int)HttpStatusCode.NotFound,
                ConflictException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            // Son kullanıcı mesajlarını daima kullanıcı dostu ve kod/numara içermeyen şekilde ver.
            string userMessage;
            if (ex is ValidationException vex)
            {
                // İlk hata mesajını yüzeye çıkar
                var first = vex.Errors?.FirstOrDefault();
                userMessage = first?.ErrorMessage ?? "Geçersiz giriş. Lütfen bilgileri kontrol edin.";
            }
            else if (ex is UnauthorizedException)
            {
                userMessage = "Oturum açmanız gerekiyor.";
            }
            else if (ex is ForbiddenException)
            {
                userMessage = "Bu işlemi yapma izniniz yok.";
            }
            else if (ex is NotFoundException)
            {
                userMessage = "Aradığınız kayıt bulunamadı.";
            }
            else if (ex is ConflictException)
            {
                userMessage = "İstek mevcut durumla çakışıyor.";
            }
            else
            {
                userMessage = "Beklenmeyen bir hata oluştu.";
            }

            var details = new ProblemDetails
            {
                // Body'de status kodunu göstermemek için null bırak (HTTP status header yine set edilecek)
                Status = null,
                Title = "İstek işlenemedi",
                Detail = userMessage,
                Instance = context.Request.Path,
            };

            details.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(details);
        }
    }
}
