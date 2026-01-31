namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record DoctorDailySlotDto(
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    bool IsAvailable,
    Guid? AppointmentId,
    string? Status
);
