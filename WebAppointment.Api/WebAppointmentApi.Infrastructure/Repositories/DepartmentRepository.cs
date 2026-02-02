using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;

    public DepartmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Department?> FindByIdAsync(int departmentId, CancellationToken ct)
        => _db.Departments.Include(x => x.Doctors).SingleOrDefaultAsync(x => x.Id == departmentId, ct);

    public async Task<IReadOnlyList<Department>> ListAsync(CancellationToken ct)
        => await _db.Departments.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Department>> ListByHospitalAsync(int hospitalId, CancellationToken ct)
        => await _db.Departments.AsNoTracking()
            .Where(x => x.HospitalId == hospitalId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    public Task AddAsync(Department department, CancellationToken ct)
    {
        _db.Departments.Add(department);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
