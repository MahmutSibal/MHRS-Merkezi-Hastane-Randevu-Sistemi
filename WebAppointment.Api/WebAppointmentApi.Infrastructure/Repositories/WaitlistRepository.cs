using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Waitlist.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class WaitlistRepository : IWaitlistRepository
{
    private readonly AppDbContext _db;

    public WaitlistRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsPendingAsync(Guid userId, int doctorId, DateOnly desiredDate, TimeOnly desiredTime, CancellationToken ct)
    {
        return _db.Waitlist.AsNoTracking().AnyAsync(x =>
            x.UserId == userId
            && x.DoctorId == doctorId
            && x.DesiredDate == desiredDate
            && x.DesiredTime == desiredTime
            && x.Status == WaitlistStatus.Pending,
            ct);
    }

    public async Task<IReadOnlyList<WaitlistEntry>> ListPendingForSlotAsync(int doctorId, DateOnly desiredDate, TimeOnly desiredTime, int take, CancellationToken ct)
    {
        return await _db.Waitlist
            .Where(x => x.DoctorId == doctorId
                && x.DesiredDate == desiredDate
                && x.DesiredTime == desiredTime
                && x.Status == WaitlistStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task AddAsync(WaitlistEntry entry, CancellationToken ct)
    {
        _db.Waitlist.Add(entry);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
