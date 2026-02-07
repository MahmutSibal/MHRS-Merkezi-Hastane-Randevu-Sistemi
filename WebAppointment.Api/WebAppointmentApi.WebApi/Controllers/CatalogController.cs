using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Departments.Abstractions;
using WebAppointmentApi.Application.Departments.Dtos;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;
using WebAppointmentApi.Application.Hospitals.Abstractions;
using WebAppointmentApi.Application.Hospitals.Dtos;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize(Roles = "Patient")]
public sealed class CatalogController : ControllerBase
{
    private readonly IDepartmentService _departments;
    private readonly IDoctorService _doctors;
    private readonly IHospitalService _hospitals;
    private readonly IPublicDoctorCalendarService _publicCalendar;

    public CatalogController(
        IDepartmentService departments,
        IDoctorService doctors,
        IHospitalService hospitals,
        IPublicDoctorCalendarService publicCalendar)
    {
        _departments = departments;
        _doctors = doctors;
        _hospitals = hospitals;
        _publicCalendar = publicCalendar;
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

    [HttpGet("doctors/{id:int}")]
    public async Task<ActionResult<DoctorPublicDetailDto>> DoctorDetail([FromRoute] int id, CancellationToken ct)
    {
        var repo = HttpContext.RequestServices.GetService<WebAppointmentApi.Application.Common.Abstractions.IDoctorRepository>();
        if (repo is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Doctor repository not available.");
        }

        var doctor = await repo.FindByIdAsync(id, ct);
        if (doctor is null || !doctor.IsActive)
        {
            return NotFound();
        }

        var isApproved = doctor.ProfileStatus == DoctorProfileStatus.Approved;
        return Ok(new DoctorPublicDetailDto(
            Id: doctor.Id,
            Name: doctor.Name,
            Title: doctor.Title,
            DepartmentId: doctor.DepartmentId,
            DepartmentName: doctor.Department?.Name ?? string.Empty,
            ProfileStatus: doctor.ProfileStatus.ToString(),
            GraduationUniversity: isApproved ? doctor.GraduationUniversity : null,
            ExperienceSummary: isApproved ? doctor.ExperienceSummary : null));
    }

    [HttpGet("doctors/{id:int}/daily-slots")]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDailySlotPublicDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorDailySlotPublicDto>>> DoctorDailySlots(
        [FromRoute] int id,
        [FromQuery] string date,
        CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var day))
        {
            return BadRequest("Geçersiz tarih.");
        }

        var slots = await _publicCalendar.GetDoctorDailySlotsAsync(id, day, ct);
        return Ok(slots);
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
