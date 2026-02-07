using FluentValidation;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Services;
using WebAppointmentApi.Application.Appointments.State;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Services;
using WebAppointmentApi.Application.Departments.Abstractions;
using WebAppointmentApi.Application.Departments.Services;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Services;
using WebAppointmentApi.Application.Notifications.Abstractions;
using WebAppointmentApi.Application.Notifications.Services;
using WebAppointmentApi.Application.Patients.Abstractions;
using WebAppointmentApi.Application.Patients.Services;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Services;
using WebAppointmentApi.Application.Audit.Abstractions;
using WebAppointmentApi.Application.Audit.Services;
using WebAppointmentApi.Application.Dependents.Abstractions;
using WebAppointmentApi.Application.Dependents.Services;
using WebAppointmentApi.Application.Waitlist.Abstractions;
using WebAppointmentApi.Application.Waitlist.Services;

namespace WebAppointmentApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILoginSecurityService, LoginSecurityService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDoctorAppointmentService, DoctorAppointmentService>();
        services.AddScoped<IDoctorCalendarService, DoctorCalendarService>();
        services.AddScoped<IPublicDoctorCalendarService, PublicDoctorCalendarService>();
        services.AddSingleton<IAppointmentStateMachine, AppointmentStateMachine>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<WebAppointmentApi.Application.Hospitals.Abstractions.IHospitalService, WebAppointmentApi.Application.Hospitals.Services.HospitalService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IWaitlistService, WaitlistService>();
        services.AddScoped<IDependentService, DependentService>();

        return services;
    }
}
