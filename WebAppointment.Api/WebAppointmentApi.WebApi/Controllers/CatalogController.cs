using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Departments.Abstractions;
using WebAppointmentApi.Application.Departments.Dtos;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;
using WebAppointmentApi.Application.Hospitals.Abstractions;
using WebAppointmentApi.Application.Hospitals.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize(Roles = "Patient")]
public sealed class CatalogController : ControllerBase
{
    private readonly IDepartmentService _departments;
    private readonly IDoctorService _doctors;
    private readonly IHospitalService _hospitals;

    public CatalogController(IDepartmentService departments, IDoctorService doctors, IHospitalService hospitals)
    {
        _departments = departments;
        _doctors = doctors;
        _hospitals = hospitals;
    }

    [HttpGet("departments")]
    public async Task<IReadOnlyList<DepartmentDto>> Departments([FromQuery] int? hospitalId, CancellationToken ct)
    {
        if (hospitalId is not null)
        {
            // list departments for a specific hospital
            var repo = HttpContext.RequestServices.GetService<WebAppointmentApi.Application.Common.Abstractions.IDepartmentRepository>();
            if (repo is null) return await _departments.ListAsync(ct);
            var entities = await repo.ListByHospitalAsync(hospitalId.Value, ct);
            return entities.Select(x => new DepartmentDto(x.Id, x.Name)).ToList();
        }
        return await _departments.ListAsync(ct);
    }

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

    [HttpGet("hospitals")]
    public async Task<IReadOnlyList<HospitalDto>> Hospitals([FromQuery] double? latitude, [FromQuery] double? longitude, [FromQuery] int? take, CancellationToken ct)
    {
        if (latitude is not null && longitude is not null)
        {
            return await _hospitals.ListNearestAsync(latitude.Value, longitude.Value, take, ct);
        }
        return await _hospitals.ListAsync(ct);
    }
}
