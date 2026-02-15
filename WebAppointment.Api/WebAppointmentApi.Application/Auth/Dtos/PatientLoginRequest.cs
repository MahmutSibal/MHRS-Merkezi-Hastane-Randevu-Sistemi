namespace WebAppointmentApi.Application.Auth.Dtos;

// Hasta girişi için TC + şifre
public sealed record PatientLoginRequest(
    string TcKimlikNo,
    string Password
);
