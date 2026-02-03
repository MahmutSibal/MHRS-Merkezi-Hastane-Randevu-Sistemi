using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Patients.Abstractions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospital/patients")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalPatientsController : ControllerBase
{
    private readonly IPatientService _patients;
    private readonly IPatientRepository _patientRepo;
    private readonly IUserRepository _users;

    public HospitalPatientsController(IPatientService patients, IPatientRepository patientRepo, IUserRepository users)
    {
        _patients = patients;
        _patientRepo = patientRepo;
        _users = users;
    }

    [HttpGet]
    public async Task<IReadOnlyList<PatientDto>> List(CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Array.Empty<PatientDto>();
        var patients = await _patientRepo.ListByHospitalAsync(hospitalId.Value, ct);
        return patients.Select(p => new PatientDto(
            p.Id,
            p.UserId,
            p.User?.Email ?? string.Empty,
            p.TcKimlikNo,
            p.FirstName,
            p.LastName,
            p.Phone)).ToList();
    }

    [HttpPost]
    public Task<PatientDto> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
        => _patients.CreateAsync(request, ct);

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientDto>> Update([FromRoute] int id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var allowed = await _patientRepo.IsPatientInHospitalAsync(id, hospitalId.Value, ct);
        if (!allowed) return Forbid();
        return await _patients.UpdateAsync(id, request, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Forbid();
        var allowed = await _patientRepo.IsPatientInHospitalAsync(id, hospitalId.Value, ct);
        if (!allowed) return Forbid();
        await _patients.DeleteAsync(id, ct);
        return NoContent();
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
}
