namespace WebAppointmentApi.Application.Notifications.Dtos;

public sealed record NotificationDto(
    long Id,
    Guid AppointmentId,
    string Message,
    DateTimeOffset CreatedAtUtc,
    bool IsSent,
    bool IsRead);
