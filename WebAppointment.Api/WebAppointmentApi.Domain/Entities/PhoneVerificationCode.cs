using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class PhoneVerificationCode : IMultiTenant
{
    public Guid Id { get; set; }

    public int TenantId { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string CodeHash { get; set; } = string.Empty;

    public string CodeSalt { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? UsedAtUtc { get; set; }

    public int AttemptCount { get; set; }
}
