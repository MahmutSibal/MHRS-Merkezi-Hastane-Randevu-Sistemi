namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record ConfirmEmailVerificationRequest(
    string Email,
    string Password,
    string Code
);
