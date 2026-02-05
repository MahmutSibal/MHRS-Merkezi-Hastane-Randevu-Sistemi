using WebAppointmentApi.Application.Audit.Abstractions;
using WebAppointmentApi.Application.Audit.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Application.Audit.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _audit;

    public AuditLogService(IAuditLogRepository audit)
    {
        _audit = audit;
    }

    public async Task<IReadOnlyList<AuditLogDto>> ListAsync(string? entity, string? action, int take, CancellationToken ct)
    {
        var list = await _audit.ListAsync(entity, action, take, ct);
        return list
            .Select(x => new AuditLogDto(
                Id: x.Id,
                Action: x.Action,
                Entity: x.Entity,
                EntityId: x.EntityId,
                Role: x.Role,
                UserId: x.UserId?.ToString(),
                IpAddress: x.IpAddress,
                TimestampUtc: x.TimestampUtc))
            .ToList();
    }
}
