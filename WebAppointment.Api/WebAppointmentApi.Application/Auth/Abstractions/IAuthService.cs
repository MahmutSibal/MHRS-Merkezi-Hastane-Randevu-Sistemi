using WebAppointmentApi.Application.Auth.Dtos;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.Application.Auth.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct);
    Task LogoutAsync(LogoutRequest request, CancellationToken ct);

    Task<LoginResponse> UpdateMyCredentialsAsync(Guid userId, UpdateMyCredentialsRequest request, CancellationToken ct);

    /// <summary>
    /// Registers a new patient user and returns tokens (login-like response).
    /// </summary>
    Task<LoginResponse> RegisterAsync(CreatePatientRequest request, CancellationToken ct);
}
