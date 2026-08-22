using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

/// <summary>Single row per tenant holding site-wide feature toggles managed by the superadmin.</summary>
public sealed class SiteSetting : IMultiTenant
{
    public int Id { get; set; }

    public bool IsSmaEnabled { get; set; } = true;

    public int TenantId { get; set; }
}
