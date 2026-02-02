using AutoMapper;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Mappings;

public sealed class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor!.Name))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Doctor!.Department!.Name))
            .ForMember(d => d.AppointmentDateUtc, o => o.MapFrom(s => s.StartAt))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<Appointment, AdminAppointmentDto>()
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User!.Email))
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor!.Name))
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Doctor!.Department!.Name))
            .ForMember(d => d.AppointmentDateUtc, o => o.MapFrom(s => s.StartAt))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
