using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly AppDbContext _db;

    public DoctorRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Doctor?> FindByIdAsync(int doctorId, CancellationToken ct)
    {
        return _db.Doctors
            .Include(x => x.Department)
            .SingleOrDefaultAsync(x => x.Id == doctorId, ct);
    }

    public Task<Doctor?> FindByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return _db.Doctors
            .Include(x => x.Department)
            .SingleOrDefaultAsync(x => x.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<Doctor>> ListAsync(CancellationToken ct)
    {
        return await _db.Doctors.AsNoTracking()
            .Include(x => x.Department)!.ThenInclude(d => d.Hospital)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public Task AddAsync(Doctor doctor, CancellationToken ct)
    {
        _db.Doctors.Add(doctor);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
