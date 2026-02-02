namespace WebAppointmentApi.Domain.Entities;

public sealed class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool IsActive { get; set; } = true;
}
