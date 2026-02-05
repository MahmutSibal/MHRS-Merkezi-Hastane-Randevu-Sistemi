using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class LoginLockout : IMultiTenant
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public string NormalizedEmail { get; set; } = string.Empty;

    public int FailedCount { get; set; }

    public DateTimeOffset FirstFailedAtUtc { get; set; }

    public DateTimeOffset LastFailedAtUtc { get; set; }

    public DateTimeOffset? LockedUntilUtc { get; set; }

    public string? LastIpAddress { get; set; }
}
