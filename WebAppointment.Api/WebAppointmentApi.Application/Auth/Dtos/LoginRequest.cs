namespace WebAppointmentApi.Application.Auth.Dtos;

// Hasta girişi için TC + şifre, yönetim/doktor için e-posta + şifre desteklenir.
public sealed record LoginRequest(
    string? Email,
    string? TcKimlikNo,
    string Password,
    string? RecaptchaToken = null
);
