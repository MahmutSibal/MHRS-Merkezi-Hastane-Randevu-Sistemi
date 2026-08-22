using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _db;

    public PatientRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Patient?> FindByTcAsync(string tcKimlikNo, CancellationToken ct)
        => _db.Patients.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TcKimlikNo == tcKimlikNo, ct);

    public Task<Patient?> FindByIdAsync(int id, CancellationToken ct)
        => _db.Patients.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<Patient?> FindByUserIdAsync(Guid userId, CancellationToken ct)
        => _db.Patients.SingleOrDefaultAsync(x => x.UserId == userId, ct);

    public Task<Patient?> FindByPhoneSuffixAsync(string last10Digits, CancellationToken ct)
        => _db.Patients.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => EF.Functions.Like(x.Phone, "%" + last10Digits), ct);

    public async Task<IReadOnlyList<Patient>> ListAsync(CancellationToken ct)
        => await _db.Patients.AsNoTracking().Include(x => x.User).OrderBy(x => x.Id).ToListAsync(ct);

    public Task AddAsync(Patient patient, CancellationToken ct)
    {
        _db.Patients.Add(patient);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Patient patient, CancellationToken ct)
    {
        patient.IsDeleted = true;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
