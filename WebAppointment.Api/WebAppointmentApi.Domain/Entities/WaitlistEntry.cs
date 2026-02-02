using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public enum WaitlistStatus { Pending = 1, Notified = 2, Confirmed = 3, Expired = 4, Cancelled = 5 }

public sealed class WaitlistEntry : IMultiTenant
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Guid UserId { get; set; }
    public int? HospitalId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DoctorId { get; set; }
    public DateOnly? DesiredDate { get; set; }
    public TimeOnly? DesiredTime { get; set; }
    public WaitlistStatus Status { get; set; } = WaitlistStatus.Pending;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? NotifiedAtUtc { get; set; }
}
