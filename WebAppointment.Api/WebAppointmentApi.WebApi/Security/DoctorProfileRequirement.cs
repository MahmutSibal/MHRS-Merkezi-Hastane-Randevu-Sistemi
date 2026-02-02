using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Security;

public sealed class DoctorProfileRequirement : IAuthorizationRequirement { }

public sealed class DoctorProfileHandler : AuthorizationHandler<DoctorProfileRequirement>
{
    private readonly IDoctorRepository _doctors;

    public DoctorProfileHandler(IDoctorRepository doctors)
    {
        _doctors = doctors;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DoctorProfileRequirement requirement)
    {
        // Role claim mapping can vary depending on JWT handler settings.
        // Support both standard ClaimTypes.Role and common JWT role claim types.
        var role = context.User.FindFirstValue(ClaimTypes.Role)
                   ?? context.User.FindFirstValue("role")
                   ?? context.User.FindFirstValue("roles")
                   ?? string.Empty;

        if (!string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Primary: JWT "sub". Fallback: NameIdentifier.
        var sub = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? context.User.FindFirstValue("sub")
                  ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(sub, out var userId))
        {
            return;
        }

        var doctor = await _doctors.FindByUserIdAsync(userId, CancellationToken.None);
        if (doctor is not null && doctor.IsActive)
        {
            context.Succeed(requirement);
        }
    }
}
