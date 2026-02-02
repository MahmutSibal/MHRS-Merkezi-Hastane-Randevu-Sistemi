using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Departments.Abstractions;
using WebAppointmentApi.Application.Departments.Dtos;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Departments.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departments;
    private readonly IHospitalRepository _hospitals;

    public DepartmentService(IDepartmentRepository departments, IHospitalRepository hospitals)
    {
        _departments = departments;
        _hospitals = hospitals;
    }

    public async Task<IReadOnlyList<DepartmentDto>> ListAsync(CancellationToken ct)
    {
        var list = await _departments.ListAsync(ct);
        return list.Select(x => new DepartmentDto(x.Id, x.Name)).ToList();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ConflictException("Department name is required.");
        }

        var hospital = await _hospitals.FindByIdAsync(request.HospitalId, ct);
        if (hospital is null)
        {
            throw new NotFoundException("Hospital not found.");
        }

        var department = new Department
        {
            Name = request.Name.Trim(),
            HospitalId = request.HospitalId,
            IsDeleted = false,
        };

        await _departments.AddAsync(department, ct);
        await _departments.SaveChangesAsync(ct);

        return new DepartmentDto(department.Id, department.Name);
    }

    public async Task<DepartmentDto> UpdateAsync(int departmentId, UpdateDepartmentRequest request, CancellationToken ct)
    {
        var dept = await _departments.FindByIdAsync(departmentId, ct);
        if (dept is null)
        {
            throw new NotFoundException("Department not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ConflictException("Department name is required.");
        }

        dept.Name = request.Name.Trim();

        await _departments.SaveChangesAsync(ct);

        return new DepartmentDto(dept.Id, dept.Name);
    }

    public async Task DeleteAsync(int departmentId, CancellationToken ct)
    {
        var dept = await _departments.FindByIdAsync(departmentId, ct);
        if (dept is null)
        {
            throw new NotFoundException("Department not found.");
        }

        dept.IsDeleted = true;
        foreach (var doctor in dept.Doctors)
        {
            doctor.IsActive = false;
        }

        await _departments.SaveChangesAsync(ct);
    }
}
