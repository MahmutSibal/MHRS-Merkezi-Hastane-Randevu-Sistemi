using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Waitlist.Abstractions;
using WebAppointmentApi.Infrastructure.BackgroundJobs;
using WebAppointmentApi.Infrastructure.Data;
using WebAppointmentApi.Infrastructure.Messaging;
using WebAppointmentApi.Infrastructure.Repositories;
using WebAppointmentApi.Infrastructure.Security;

namespace WebAppointmentApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var cs = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(cs);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();
        services.AddScoped<IDoctorTimeOffRepository, DoctorTimeOffRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IHospitalRepository, HospitalRepository>();
            services.AddScoped<IDependentRepository, DependentRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ILoginLockoutRepository, LoginLockoutRepository>();
        services.AddScoped<IWaitlistRepository, WaitlistRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IPhoneVerificationRepository, PhoneVerificationRepository>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
        services.AddHostedService<QueuedBackgroundService>();

        services.Configure<WhatsAppBridgeOptions>(configuration.GetSection("WhatsAppBridge"));
        services.AddHttpClient<IPhoneVerificationSender, WhatsAppPhoneVerificationSender>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppBridgeOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}
