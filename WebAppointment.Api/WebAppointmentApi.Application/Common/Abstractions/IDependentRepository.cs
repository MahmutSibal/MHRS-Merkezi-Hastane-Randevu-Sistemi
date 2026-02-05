using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IDependentRepository
{
    Task<Dependent?> FindByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Dependent>> ListByGuardianUserIdAsync(Guid guardianUserId, CancellationToken ct);
    Task AddAsync(Dependent dependent, CancellationToken ct);
    Task DeleteAsync(Dependent dependent, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
