using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class LoginLockoutRepository : ILoginLockoutRepository
{
    private readonly AppDbContext _db;

    public LoginLockoutRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<LoginLockout?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken ct)
        => _db.Set<LoginLockout>().FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, ct);

    public Task AddAsync(LoginLockout entity, CancellationToken ct)
        => _db.Set<LoginLockout>().AddAsync(entity, ct).AsTask();

    public void Remove(LoginLockout entity)
        => _db.Set<LoginLockout>().Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
