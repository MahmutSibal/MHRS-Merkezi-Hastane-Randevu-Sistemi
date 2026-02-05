namespace WebAppointmentApi.Application.Auth.Abstractions;

public interface ILoginSecurityService
{
    Task EnsureNotLockedAsync(string normalizedEmail, CancellationToken ct);

    Task RegisterFailureAsync(string normalizedEmail, string? ipAddress, CancellationToken ct);

    Task RegisterSuccessAsync(string normalizedEmail, CancellationToken ct);
}
