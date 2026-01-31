using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Departments.Abstractions;
using WebAppointmentApi.Application.Departments.Dtos;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize(Roles = "Patient")]
public sealed class CatalogController : ControllerBase
{
    private readonly IDepartmentService _departments;
    private readonly IDoctorService _doctors;

    public CatalogController(IDepartmentService departments, IDoctorService doctors)
    {
        _departments = departments;
        _doctors = doctors;
    }

    [HttpGet("departments")]
    public Task<IReadOnlyList<DepartmentDto>> Departments(CancellationToken ct)
        => _departments.ListAsync(ct);

    [HttpGet("doctors")]
    public async Task<IReadOnlyList<DoctorDto>> Doctors([FromQuery] int? departmentId, CancellationToken ct)
    {
        var list = await _doctors.ListAsync(ct);
        return list
            .Where(x => x.IsActive)
            .Where(x => departmentId is null || x.DepartmentId == departmentId)
            .Select(x => x with { UserId = null })
            .ToList();
    }
}
