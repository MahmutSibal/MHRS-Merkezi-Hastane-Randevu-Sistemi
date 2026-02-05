using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> ListAsync(string? entity, string? action, int take, CancellationToken ct);
}
