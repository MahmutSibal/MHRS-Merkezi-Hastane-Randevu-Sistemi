using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IUserContext _user;

    public AuthController(IAuthService auth, IUserContext user)
    {
        _auth = auth;
        _user = user;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public Task<LoginResponse> Register([FromBody] CreatePatientRequest request, CancellationToken ct)
        => _auth.RegisterAsync(request, ct);

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public Task<LoginResponse> Login([FromBody] LoginRequest request, CancellationToken ct)
        => _auth.LoginAsync(request, ct);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<LoginResponse> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
        => _auth.RefreshAsync(request, ct);

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request, ct);
        return NoContent();
    }

    [HttpPatch("me/credentials")]
    [Authorize]
    public Task<LoginResponse> UpdateMyCredentials([FromBody] UpdateMyCredentialsRequest request, CancellationToken ct)
        => _auth.UpdateMyCredentialsAsync(_user.UserId, request, ct);
}
