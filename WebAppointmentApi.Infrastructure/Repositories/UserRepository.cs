using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    // Avoid Turkish-I casing issues by forcing a stable CI collation for email comparisons.
    // This collation name is available on SQL Server and makes comparisons case-insensitive.
    private const string EmailCollation = "SQL_Latin1_General_CP1_CI_AS";

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        var email = (normalizedEmail ?? string.Empty).Trim().ToUpperInvariant();

        return _db.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(
                x => EF.Functions.Collate(x.Email, EmailCollation) == email,
                ct);
    }

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct)
    {
        return _db.Users.SingleOrDefaultAsync(x => x.Id == userId, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct)
        => await _db.Users.AsNoTracking().AnyAsync(u => u.Id == id, ct);

    public Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken ct)
    {
        return _db.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
    }

    public Task AddAsync(User user, CancellationToken ct)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct)
    {
        _db.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
