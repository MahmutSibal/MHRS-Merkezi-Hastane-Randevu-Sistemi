namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record UpdateMyCredentialsRequest(
    string CurrentPassword,
    string? NewEmail,
    string? NewPassword);
