using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class DependentRepository : IDependentRepository
{
    private readonly AppDbContext _db;

    public DependentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Dependent?> FindByIdAsync(int id, CancellationToken ct)
        => _db.Set<Dependent>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Dependent>> ListByGuardianUserIdAsync(Guid guardianUserId, CancellationToken ct)
        => await _db.Set<Dependent>().AsNoTracking().Where(x => x.GuardianUserId == guardianUserId).ToListAsync(ct);

    public Task AddAsync(Dependent dependent, CancellationToken ct)
    {
        _db.Set<Dependent>().Add(dependent);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Dependent dependent, CancellationToken ct)
    {
        _db.Set<Dependent>().Remove(dependent);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
