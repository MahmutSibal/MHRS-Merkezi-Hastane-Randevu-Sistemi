using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class AuditLog : IMultiTenant
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Created / Updated / Deleted
    public string Entity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Before { get; set; }
    public string? After { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public string? IpAddress { get; set; }
}
