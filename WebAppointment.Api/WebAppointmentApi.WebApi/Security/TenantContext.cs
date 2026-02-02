using System.Security.Claims;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Security;

public sealed class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _http;
    private readonly IConfiguration _config;

    public TenantContext(IHttpContextAccessor http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public int TenantId
    {
        get
        {
            var principal = _http.HttpContext?.User;
            var claim = principal?.FindFirst("tenant_id")?.Value;
            if (int.TryParse(claim, out var id) && id > 0)
            {
                return id;
            }
            var fromHeader = _http.HttpContext?.Request?.Headers["X-Tenant-Id"].FirstOrDefault();
            if (int.TryParse(fromHeader, out id) && id > 0)
            {
                return id;
            }
            // fallback default
            var fallback = _config["MultiTenancy:DefaultTenantId"];
            return int.TryParse(fallback, out id) && id > 0 ? id : 1;
        }
    }
}
