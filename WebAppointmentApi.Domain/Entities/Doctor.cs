namespace WebAppointmentApi.Domain.Entities;

public sealed class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public List<Appointment> Appointments { get; set; } = new();
}
