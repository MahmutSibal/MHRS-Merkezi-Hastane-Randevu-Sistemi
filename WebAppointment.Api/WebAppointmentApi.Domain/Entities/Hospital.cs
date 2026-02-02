using WebAppointmentApi.Domain.Common;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Hospital : IMultiTenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public HospitalType Type { get; set; }
    public bool IsDeleted { get; set; }

    public int TenantId { get; set; }

    public List<Department> Departments { get; set; } = new();
}
