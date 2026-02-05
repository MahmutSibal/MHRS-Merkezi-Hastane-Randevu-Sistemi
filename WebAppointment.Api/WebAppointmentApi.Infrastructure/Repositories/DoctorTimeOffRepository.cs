using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class DoctorTimeOffRepository : IDoctorTimeOffRepository
{
    private readonly AppDbContext _db;

    public DoctorTimeOffRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DoctorTimeOff>> ListForDoctorBetweenAsync(int doctorId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct)
    {
        return await _db.Set<DoctorTimeOff>().AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.StartAtUtc < toUtc && x.EndAtUtc > fromUtc)
            .OrderBy(x => x.StartAtUtc)
            .ToListAsync(ct);
    }

    public Task AddAsync(DoctorTimeOff timeOff, CancellationToken ct)
    {
        _db.Set<DoctorTimeOff>().Add(timeOff);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
