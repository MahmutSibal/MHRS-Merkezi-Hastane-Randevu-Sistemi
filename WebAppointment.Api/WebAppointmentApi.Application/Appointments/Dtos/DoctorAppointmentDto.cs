namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record DoctorAppointmentDto(
    Guid Id,
    Guid PatientUserId,
    string PatientEmail,
    int DoctorId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    string Status
);
