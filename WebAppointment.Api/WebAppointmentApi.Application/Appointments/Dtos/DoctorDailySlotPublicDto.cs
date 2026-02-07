namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record DoctorDailySlotPublicDto(
    string StartTime,
    string EndTime,
    bool IsAvailable,
    string? UnavailableReason
);
