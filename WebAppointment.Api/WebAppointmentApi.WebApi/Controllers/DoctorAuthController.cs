using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/auth/doctor")]
public sealed class DoctorAuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public DoctorAuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // Doktor giriş: e-posta + şifre, rol kontrolü Doctor olmalı
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
        if (!string.Equals(response.Role, "Doctor", StringComparison.OrdinalIgnoreCase))
        {
            // Yanlış rol için tokenı hemen geçersiz say (refresh'i revoke et)
            await _auth.LogoutAsync(new LogoutRequest(response.RefreshToken), ct);
            return Unauthorized(new { message = "Bu giriş sadece Doktor rolü için geçerlidir." });
        }

        return Ok(response);
    }

    // Doktor kayıt: sistem/hastane yöneticisi tarafından yönetilir
    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register()
        => StatusCode(StatusCodes.Status405MethodNotAllowed, new { message = "Doktor kayıtları hastane yönetimi tarafından oluşturulur." });
}
