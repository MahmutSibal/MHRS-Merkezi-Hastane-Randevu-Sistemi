using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<User?> FindByIdAsync(Guid userId, CancellationToken ct);
    Task<bool> ExistsByIdAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<User>> ListHospitalAdminsByHospitalIdAsync(int hospitalId, CancellationToken ct);
    Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task DeleteAsync(User user, CancellationToken ct);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
