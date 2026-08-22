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

    Task RequestPhoneVerificationCodeAsync(PhoneVerificationRequest request, CancellationToken ct);
    Task<LoginResponse> RegisterWithPhoneVerificationAsync(RegisterWithPhoneVerificationRequest request, CancellationToken ct);

    Task ForgotPatientPasswordAsync(PatientForgotPasswordRequest request, CancellationToken ct);

    /// <summary>(Re)sends an email verification code to a Doctor/Admin/HospitalAdmin account.</summary>
    Task RequestEmailVerificationCodeAsync(EmailVerificationRequest request, CancellationToken ct);

    /// <summary>Confirms the emailed code, marks the account verified, and logs the user in.</summary>
    Task<LoginResponse> ConfirmEmailVerificationAsync(ConfirmEmailVerificationRequest request, CancellationToken ct);

    /// <summary>Logs in an existing Doctor/Admin/HospitalAdmin account via a validated Google ID token.</summary>
    Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken ct);
}
