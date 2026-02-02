using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Hospitals.Dtos;

public sealed record HospitalDto(
    int Id,
    string Name,
    string? Address,
    double? Latitude,
    double? Longitude,
    HospitalType Type
);
