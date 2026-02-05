using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Auth.Abstractions;

public interface ILoginLockoutRepository
{
    Task<LoginLockout?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken ct);

    Task AddAsync(LoginLockout entity, CancellationToken ct);

    void Remove(LoginLockout entity);

    Task SaveChangesAsync(CancellationToken ct);
}
