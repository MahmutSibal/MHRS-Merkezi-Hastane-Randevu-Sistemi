namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record EmailVerificationRequest(
    string Email,
    string Password
);
