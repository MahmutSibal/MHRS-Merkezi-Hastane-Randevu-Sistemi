namespace WebAppointmentApi.Domain.Entities;

public sealed class Notification
{
    public long Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public bool IsSent { get; set; }
}
