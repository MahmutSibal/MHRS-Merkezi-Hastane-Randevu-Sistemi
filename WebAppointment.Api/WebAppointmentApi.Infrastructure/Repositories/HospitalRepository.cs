using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class HospitalRepository : IHospitalRepository
{
    private readonly AppDbContext _db;

    public HospitalRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Hospital?> FindByIdAsync(int hospitalId, CancellationToken ct)
        => _db.Hospitals.Include(x => x.Departments).SingleOrDefaultAsync(x => x.Id == hospitalId, ct);

    public async Task<IReadOnlyList<Hospital>> ListAsync(CancellationToken ct)
        => await _db.Hospitals.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public Task AddAsync(Hospital hospital, CancellationToken ct)
    {
        _db.Hospitals.Add(hospital);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
