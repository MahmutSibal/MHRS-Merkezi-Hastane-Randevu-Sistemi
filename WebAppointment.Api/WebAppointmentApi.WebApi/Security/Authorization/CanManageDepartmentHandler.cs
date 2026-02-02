using Microsoft.AspNetCore.Authorization;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Security.Authorization;

public sealed class CanManageDepartmentHandler : AuthorizationHandler<CanManageDepartmentRequirement>
{
    private readonly IHttpContextAccessor _http;
    private readonly IDepartmentRepository _departments;
    private readonly IUserContext _user;

    public CanManageDepartmentHandler(IHttpContextAccessor http, IDepartmentRepository departments, IUserContext user)
    {
        _http = http;
        _departments = departments;
        _user = user;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CanManageDepartmentRequirement requirement)
    {
        // Only Admin/HospitalAdmin are eligible
        if (!string.Equals(_user.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(_user.Role, "HospitalAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return; // not fulfilled
        }

        var http = _http.HttpContext;
        if (http is null)
        {
            return;
        }

        // Try get department id from route values: id or departmentId
        int departmentId = 0;
        if (http.Request.RouteValues.TryGetValue("id", out var idVal) && int.TryParse(Convert.ToString(idVal), out var rid))
        {
            departmentId = rid;
        }
        else if (http.Request.RouteValues.TryGetValue("departmentId", out var didVal) && int.TryParse(Convert.ToString(didVal), out var did))
        {
            departmentId = did;
        }

        if (departmentId <= 0)
        {
            // No explicit resource – allow, controllers should still use tenant filters
            context.Succeed(requirement);
            return;
        }

        var dept = await _departments.FindByIdAsync(departmentId, http.RequestAborted);
        if (dept is null)
        {
            return;
        }

        // Tenant match enforcement
        if (dept.TenantId == _user.TenantId)
        {
            context.Succeed(requirement);
        }
    }
}
