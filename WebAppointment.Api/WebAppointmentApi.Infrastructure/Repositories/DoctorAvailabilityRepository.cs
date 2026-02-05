using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class DoctorAvailabilityRepository : IDoctorAvailabilityRepository
{
    private readonly AppDbContext _db;

    public DoctorAvailabilityRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<DoctorAvailability?> FindByDoctorIdAsync(int doctorId, CancellationToken ct)
    {
        return _db.Set<DoctorAvailability>().SingleOrDefaultAsync(x => x.DoctorId == doctorId, ct);
    }

    public async Task UpsertAsync(DoctorAvailability availability, CancellationToken ct)
    {
        var existing = await _db.Set<DoctorAvailability>().SingleOrDefaultAsync(x => x.DoctorId == availability.DoctorId, ct);
        if (existing is null)
        {
            _db.Set<DoctorAvailability>().Add(availability);
            return;
        }

        existing.WorkStart = availability.WorkStart;
        existing.WorkEnd = availability.WorkEnd;
        existing.LunchStart = availability.LunchStart;
        existing.LunchEnd = availability.LunchEnd;
        existing.SlotMinutes = availability.SlotMinutes;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
