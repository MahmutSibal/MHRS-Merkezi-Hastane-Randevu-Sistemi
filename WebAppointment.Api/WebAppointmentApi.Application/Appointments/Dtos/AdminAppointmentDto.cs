namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record AdminAppointmentDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    int DoctorId,
    string DoctorName,
    string DepartmentName,
    DateTimeOffset AppointmentDateUtc,
    string Status
);
