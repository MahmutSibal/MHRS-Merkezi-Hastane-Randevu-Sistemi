namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    Guid UserId,
    string Email,
    string Role
);
