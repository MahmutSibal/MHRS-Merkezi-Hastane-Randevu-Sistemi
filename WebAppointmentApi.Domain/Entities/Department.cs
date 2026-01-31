namespace WebAppointmentApi.Domain.Entities;

public sealed class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public List<Doctor> Doctors { get; set; } = new();
}
