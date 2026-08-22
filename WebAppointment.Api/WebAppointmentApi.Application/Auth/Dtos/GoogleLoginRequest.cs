namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record GoogleLoginRequest(
    string IdToken
);
