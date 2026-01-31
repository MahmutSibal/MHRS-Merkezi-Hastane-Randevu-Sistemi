using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
}
