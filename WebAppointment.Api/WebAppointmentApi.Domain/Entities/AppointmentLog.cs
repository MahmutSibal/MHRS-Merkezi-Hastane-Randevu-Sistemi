using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class AppointmentLog : IMultiTenant
{
    public long Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }

    public int TenantId { get; set; }
}
