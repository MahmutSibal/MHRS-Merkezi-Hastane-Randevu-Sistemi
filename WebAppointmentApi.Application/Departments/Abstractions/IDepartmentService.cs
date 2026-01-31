using WebAppointmentApi.Application.Departments.Dtos;

namespace WebAppointmentApi.Application.Departments.Abstractions;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> ListAsync(CancellationToken ct);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken ct);
    Task<DepartmentDto> UpdateAsync(int departmentId, UpdateDepartmentRequest request, CancellationToken ct);
    Task DeleteAsync(int departmentId, CancellationToken ct);
}
