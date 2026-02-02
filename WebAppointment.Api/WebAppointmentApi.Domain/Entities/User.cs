using WebAppointmentApi.Domain.Enums;

using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class User : IMultiTenant
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public int TenantId { get; set; }

    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
}
