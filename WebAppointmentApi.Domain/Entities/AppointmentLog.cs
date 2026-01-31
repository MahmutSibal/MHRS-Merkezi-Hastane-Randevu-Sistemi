namespace WebAppointmentApi.Domain.Entities;

public sealed class AppointmentLog
{
    public long Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
