using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Data;

/// <summary>
/// Design-time DbContext factory so that EF CLI can run without needing the startup project.
/// Usage: from the Infrastructure folder, run: dotnet ef migrations add YourMigrationName and dotnet ef database update.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Resolve WebApi appsettings path (one level up, then into WebApi)
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "WebAppointmentApi.WebApi"));

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? config["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' could not be found.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
            });

        // Provide a simple tenant context for design-time (default tenant id)
        var defaultTenantId = config.GetValue<int>("MultiTenancy:DefaultTenantId", 1);
        var tenant = new DesignTenantContext(defaultTenantId);

        // user context is optional in AppDbContext constructor; pass null for design-time
        return new AppDbContext(optionsBuilder.Options, tenant, user: null);
    }

    private sealed class DesignTenantContext : ITenantContext
    {
        public DesignTenantContext(int tenantId) => TenantId = tenantId;
        public int TenantId { get; }
    }
}
