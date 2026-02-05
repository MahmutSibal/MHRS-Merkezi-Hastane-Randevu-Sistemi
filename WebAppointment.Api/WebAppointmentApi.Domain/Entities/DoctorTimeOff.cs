using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class DoctorTimeOff : IMultiTenant
{
    public long Id { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }

    public string? Reason { get; set; }

    public int TenantId { get; set; }
}
