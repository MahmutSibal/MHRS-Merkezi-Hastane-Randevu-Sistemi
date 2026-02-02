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
using WebAppointmentApi.Application.Patients.Abstractions;
using WebAppointmentApi.Application.Patients.Services;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Services;

namespace WebAppointmentApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDoctorAppointmentService, DoctorAppointmentService>();
        services.AddScoped<IDoctorCalendarService, DoctorCalendarService>();
        services.AddSingleton<IAppointmentStateMachine, AppointmentStateMachine>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<WebAppointmentApi.Application.Hospitals.Abstractions.IHospitalService, WebAppointmentApi.Application.Hospitals.Services.HospitalService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
