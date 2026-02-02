namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record LoginRequest(
    string Email,
    string Password
);
