using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IDepartmentRepository
{
    Task<Department?> FindByIdAsync(int departmentId, CancellationToken ct);
    Task<IReadOnlyList<Department>> ListAsync(CancellationToken ct);
    Task AddAsync(Department department, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
