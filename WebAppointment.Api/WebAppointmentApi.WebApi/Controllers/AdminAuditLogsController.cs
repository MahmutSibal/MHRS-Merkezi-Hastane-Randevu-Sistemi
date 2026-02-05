using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Audit.Abstractions;
using WebAppointmentApi.Application.Audit.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public sealed class AdminAuditLogsController : ControllerBase
{
    private readonly IAuditLogService _audit;

    public AdminAuditLogsController(IAuditLogService audit)
    {
        _audit = audit;
    }

    [HttpGet]
    public Task<IReadOnlyList<AuditLogDto>> List([FromQuery] string? entity, [FromQuery] string? action, [FromQuery] int take = 100, CancellationToken ct = default)
        => _audit.ListAsync(entity, action, take, ct);
}
