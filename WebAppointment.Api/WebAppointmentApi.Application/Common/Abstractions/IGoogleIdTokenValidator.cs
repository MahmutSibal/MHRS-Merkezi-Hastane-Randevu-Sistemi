namespace WebAppointmentApi.Application.Common.Abstractions;

public sealed record GoogleIdentity(string Email, string? GivenName, string? FamilyName);

public interface IGoogleIdTokenValidator
{
    /// <summary>Validates a Google Identity Services ID token; returns null if invalid/expired/audience mismatch.</summary>
    Task<GoogleIdentity?> ValidateAsync(string idToken, CancellationToken ct);
}
