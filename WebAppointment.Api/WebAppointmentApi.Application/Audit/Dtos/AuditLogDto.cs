namespace WebAppointmentApi.Application.Audit.Dtos;

public sealed record AuditLogDto(
    long Id,
    string Action,
    string Entity,
    string EntityId,
    string Role,
    string? UserId,
    string? IpAddress,
    DateTimeOffset TimestampUtc);
