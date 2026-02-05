using WebAppointmentApi.Application.Audit.Dtos;

namespace WebAppointmentApi.Application.Audit.Abstractions;

public interface IAuditLogService
{
    Task<IReadOnlyList<AuditLogDto>> ListAsync(string? entity, string? action, int take, CancellationToken ct);
}
