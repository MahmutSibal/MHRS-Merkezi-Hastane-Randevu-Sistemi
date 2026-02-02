using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Doctor : IMultiTenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public int TenantId { get; set; }

    // Efficiency score for slot optimization (0-100, optional)
    public int? EfficiencyScore { get; set; }

    public List<Appointment> Appointments { get; set; } = new();
}
