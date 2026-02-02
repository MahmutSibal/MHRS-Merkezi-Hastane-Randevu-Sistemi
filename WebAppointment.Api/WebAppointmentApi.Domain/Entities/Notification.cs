using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Notification : IMultiTenant
{
    public long Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public bool IsSent { get; set; }

    public int TenantId { get; set; }
}
