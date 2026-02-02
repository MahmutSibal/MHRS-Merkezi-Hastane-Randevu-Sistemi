using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Departments.Dtos;
using WebAppointmentApi.Application.Departments.Abstractions;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospitaladmin/departments")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalAdminDepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departments;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IUserRepository _users;

    public HospitalAdminDepartmentsController(IDepartmentService departments, IDepartmentRepository departmentRepo, IUserRepository users)
    {
        _departments = departments;
        _departmentRepo = departmentRepo;
        _users = users;
    }

    private async Task<int?> GetCurrentHospitalIdAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var user = await _users.FindByIdAsync(userId, ct);
            return user?.HospitalId;
        }
        return null;
    }

    [HttpGet]
    public async Task<IReadOnlyList<DepartmentDto>> List(CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Array.Empty<DepartmentDto>();
        var entities = await _departmentRepo.ListByHospitalAsync(hospitalId.Value, ct);
        return entities.Select(x => new DepartmentDto(x.Id, x.Name)).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentRequestBody request, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var dto = await _departments.CreateAsync(new CreateDepartmentRequest(request.Name, hospitalId.Value), ct);
        return dto;
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageDepartment")]
    public async Task<ActionResult<DepartmentDto>> Update([FromRoute] int id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var dept = await _departmentRepo.FindByIdAsync(id, ct);
        if (dept is null || dept.HospitalId != hospitalId.Value) return Forbid();
        var dto = await _departments.UpdateAsync(id, request, ct);
        return dto;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageDepartment")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var dept = await _departmentRepo.FindByIdAsync(id, ct);
        if (dept is null || dept.HospitalId != hospitalId.Value) return Forbid();
        await _departments.DeleteAsync(id, ct);
        return NoContent();
    }
}

public sealed record CreateDepartmentRequestBody(string Name);
