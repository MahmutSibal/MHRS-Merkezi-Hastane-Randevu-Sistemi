using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Dtos;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/auth/patient")]
public sealed class PatientAuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public PatientAuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // Hasta kayıt: e-posta tamamen kaldırıldı
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public Task<LoginResponse> Register([FromBody] CreatePatientNoEmailRequest request, CancellationToken ct)
    {
        var mapped = new CreatePatientRequest(
            Email: null,
            Password: request.Password,
            TcKimlikNo: request.TcKimlikNo,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Phone: request.Phone
        );
        return _auth.RegisterAsync(mapped, ct);
    }

    [HttpPost("register/request-code")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPhoneCode([FromBody] PhoneVerificationRequest request, CancellationToken ct)
    {
        await _auth.RequestPhoneVerificationCodeAsync(request, ct);
        return NoContent();
    }

    [HttpPost("register/confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public Task<LoginResponse> RegisterConfirm([FromBody] RegisterWithPhoneVerificationRequest request, CancellationToken ct)
        => _auth.RegisterWithPhoneVerificationAsync(request, ct);

    // Hasta giriş: TC + şifre
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<LoginResponse> Login([FromBody] PatientLoginRequest request, CancellationToken ct)
    {
        var mapped = new LoginRequest(
            Email: null,
            TcKimlikNo: request.TcKimlikNo,
            Password: request.Password
        );
        return _auth.LoginAsync(mapped, ct);
    }
}
