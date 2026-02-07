using WebAppointmentApi.Domain.Common;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Dependent : IMultiTenant
{
    public int Id { get; set; }

    public Guid GuardianUserId { get; set; }
    public User? GuardianUser { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public DependentRelation Relation { get; set; }

    // Turkish National ID (TCKN)
    public string TcKimlikNo { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public int TenantId { get; set; }
}
