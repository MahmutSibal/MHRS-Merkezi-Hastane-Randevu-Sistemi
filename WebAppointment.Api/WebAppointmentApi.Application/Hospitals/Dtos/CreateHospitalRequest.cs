using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Hospitals.Dtos;

public sealed record CreateHospitalRequest(
    string Name,
    string? Address,
    double? Latitude,
    double? Longitude,
    HospitalType Type
);
