using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Department : IMultiTenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public int TenantId { get; set; }

    public int HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public List<Doctor> Doctors { get; set; } = new();
}
