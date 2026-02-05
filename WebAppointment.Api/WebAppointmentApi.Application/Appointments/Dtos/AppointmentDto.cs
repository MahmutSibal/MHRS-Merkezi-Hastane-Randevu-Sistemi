namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record AppointmentDto(
    Guid Id,
    Guid UserId,
    int DoctorId,
    string DoctorName,
    string DepartmentName,
    DateTimeOffset AppointmentDateUtc,
    string Status,
    int? DependentId,
    string? DependentFullName
);
