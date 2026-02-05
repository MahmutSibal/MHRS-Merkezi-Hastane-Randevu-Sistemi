using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class HolidayRepository : IHolidayRepository
{
    private readonly AppDbContext _db;

    public HolidayRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(DateOnly date, CancellationToken ct)
    {
        return _db.Set<Holiday>().AsNoTracking().AnyAsync(x => x.Date == date, ct);
    }

    public async Task<IReadOnlyList<Holiday>> ListAsync(int take, CancellationToken ct)
    {
        return await _db.Set<Holiday>().AsNoTracking()
            .OrderByDescending(x => x.Date)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task AddAsync(Holiday holiday, CancellationToken ct)
    {
        _db.Set<Holiday>().Add(holiday);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
