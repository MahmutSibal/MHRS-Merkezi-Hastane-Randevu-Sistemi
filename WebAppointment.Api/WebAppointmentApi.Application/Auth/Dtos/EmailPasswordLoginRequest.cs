namespace WebAppointmentApi.Application.Auth.Dtos;

// Yönetim ve doktor girişleri için e-posta + şifre
public sealed record EmailPasswordLoginRequest(
    string Email,
    string Password,
    string? RecaptchaToken = null
);
