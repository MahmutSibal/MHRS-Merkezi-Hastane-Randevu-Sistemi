using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Security;

public sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _http;

    public UserContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid UserId
    {
        get
        {
            var principal = _http.HttpContext?.User;
            if (principal is null)
            {
                return Guid.Empty;
            }

            // Primary: JWT "sub" claim.
            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (Guid.TryParse(sub, out var id))
            {
                return id;
            }

            // Fallbacks: common claim types.
            var nameId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(nameId, out id))
            {
                return id;
            }

            var rawSub = principal.FindFirstValue("sub");
            return Guid.TryParse(rawSub, out id) ? id : Guid.Empty;
        }
    }

    public string Role => _http.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public string Email => _http.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
}
