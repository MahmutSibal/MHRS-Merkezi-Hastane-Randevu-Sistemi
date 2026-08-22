using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/auth/admin")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AdminAuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // Sistem yöneticisi giriş: e-posta + şifre, rol kontrolü Admin olmalı
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
        if (!string.Equals(response.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            await _auth.LogoutAsync(new LogoutRequest(response.RefreshToken), ct);
            return Unauthorized(new { message = "Bu giriş sadece Admin rolü için geçerlidir." });
        }

        return Ok(response);
    }

    // Admin kayıt: dev/hastane kurulumu kapsamında yönetilir
    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register()
        => StatusCode(StatusCodes.Status405MethodNotAllowed, new { message = "Admin kayıtları sistem kurulumu kapsamında yönetilir." });
}
