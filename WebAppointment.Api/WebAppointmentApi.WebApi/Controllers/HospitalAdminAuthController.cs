using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/auth/hospital-admin")]
public sealed class HospitalAdminAuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public HospitalAdminAuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // Hastane yöneticisi giriş: e-posta + şifre, rol kontrolü HospitalAdmin olmalı
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] EmailPasswordLoginRequest request, CancellationToken ct)
    {
        var mapped = new LoginRequest(
            Email: request.Email,
            TcKimlikNo: null,
            Password: request.Password,
            RecaptchaToken: request.RecaptchaToken
        );

        var response = await _auth.LoginAsync(mapped, ct);
        if (!string.Equals(response.Role, "HospitalAdmin", StringComparison.OrdinalIgnoreCase))
        {
            await _auth.LogoutAsync(new LogoutRequest(response.RefreshToken), ct);
            return Unauthorized(new { message = "Bu giriş sadece HospitalAdmin rolü için geçerlidir." });
        }

        return Ok(response);
    }

    // HospitalAdmin kayıt: hastane yönetimi tarafından yönetilir
    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register()
        => StatusCode(StatusCodes.Status405MethodNotAllowed, new { message = "Hastane yönetici kayıtları yönetim tarafından oluşturulur." });
}
