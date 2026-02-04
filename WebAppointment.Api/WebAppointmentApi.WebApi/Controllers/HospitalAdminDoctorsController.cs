using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospitaladmin/doctors")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalAdminDoctorsController : ControllerBase
{
    private readonly IDoctorService _doctors;
    private readonly IDepartmentRepository _departments;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IUserRepository _users;

    public HospitalAdminDoctorsController(IDoctorService doctors, IDepartmentRepository departments, IUserRepository users, IDoctorRepository doctorRepo)
    {
        _doctors = doctors;
        _departments = departments;
        _users = users;
        _doctorRepo = doctorRepo;
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
    public async Task<IReadOnlyList<DoctorDto>> List(CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Array.Empty<DoctorDto>();
        var entities = await _doctorRepo.ListAsync(ct);
        var filtered = entities.Where(x => x.Department?.HospitalId == hospitalId.Value).ToList();
        return filtered.Select(x => new DoctorDto(x.Id, x.Name, x.Title, x.DepartmentId, x.Department?.Name ?? string.Empty, x.IsActive, x.UserId)).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<DoctorDto>> Create([FromBody] CreateDoctorRequest request, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var dept = await _departments.FindByIdAsync(request.DepartmentId, ct);
        if (dept is null || dept.HospitalId != hospitalId.Value) return Forbid();
        var dto = await _doctors.CreateAsync(request, ct);
        return dto;
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageDepartment")]
    public async Task<ActionResult<DoctorDto>> Update([FromRoute] int id, [FromBody] UpdateDoctorRequest request, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var dept = await _departments.FindByIdAsync(request.DepartmentId, ct);
        if (dept is null || dept.HospitalId != hospitalId.Value) return Forbid();
        var dto = await _doctors.UpdateAsync(id, request, ct);
        return dto;
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageDepartment")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        // Ensure doctor belongs to hospital via department check
        var list = await _doctors.ListAsync(ct);
        var doc = list.SingleOrDefault(x => x.Id == id);
        if (doc is null) return NotFound();
        var dept = await _departments.FindByIdAsync(doc.DepartmentId, ct);
        if (dept is null || dept.HospitalId != hospitalId.Value) return Forbid();
        await _doctors.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/credentials")]
    [Authorize(Policy = "CanManageDepartment")]
    public async Task<IActionResult> UpdateCredentials([FromRoute] int id, [FromBody] UpdateDoctorCredentialsRequest request, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var list = await _doctors.ListAsync(ct);
        var doc = list.SingleOrDefault(x => x.Id == id);
        if (doc is null) return NotFound();
        var dept = await _departments.FindByIdAsync(doc.DepartmentId, ct);
        if (dept is null || dept.HospitalId != hospitalId.Value) return Forbid();
        await _doctors.UpdateCredentialsAsync(id, request, ct);
        return NoContent();
    }
}
