using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Holiday : IMultiTenant
{
    public int Id { get; set; }

    // Local (Turkey) date
    public DateOnly Date { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TenantId { get; set; }
}
