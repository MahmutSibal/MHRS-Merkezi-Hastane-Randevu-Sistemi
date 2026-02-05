using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospitaladmin/doctor-profiles")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalAdminDoctorProfilesController : ControllerBase
{
    private readonly IDoctorRepository _doctors;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;

    public HospitalAdminDoctorProfilesController(
        IDoctorRepository doctors,
        IUserRepository users,
        IDateTimeProvider clock)
    {
        _doctors = doctors;
        _users = users;
        _clock = clock;
    }

    private async Task<(Guid UserId, int? HospitalId)> GetCurrentAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return (Guid.Empty, null);
        }
        var user = await _users.FindByIdAsync(userId, ct);
        return (userId, user?.HospitalId);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<HospitalAdminPendingDoctorProfileDto>>> ListPending(CancellationToken ct)
    {
        var (_, hospitalId) = await GetCurrentAsync(ct);
        if (hospitalId is null)
        {
            return Ok(Array.Empty<HospitalAdminPendingDoctorProfileDto>());
        }

        var list = await _doctors.ListAsync(ct);
        var filtered = list
            .Where(x => x.Department?.HospitalId == hospitalId.Value)
            .Where(x => x.ProfileStatus == DoctorProfileStatus.Pending)
            .Where(x => !string.IsNullOrWhiteSpace(x.GraduationUniversity))
            .Where(x => !string.IsNullOrWhiteSpace(x.ExperienceSummary))
            .OrderByDescending(x => x.ProfileSubmittedAtUtc)
            .Select(x => new HospitalAdminPendingDoctorProfileDto(
                DoctorId: x.Id,
                DoctorName: x.Name,
                DoctorTitle: x.Title,
                DepartmentId: x.DepartmentId,
                DepartmentName: x.Department?.Name ?? string.Empty,
                GraduationUniversity: x.GraduationUniversity!,
                ExperienceSummary: x.ExperienceSummary!,
                SubmittedAtUtc: x.ProfileSubmittedAtUtc ?? DateTimeOffset.MinValue))
            .ToList();

        return Ok(filtered);
    }

    [HttpPost("{doctorId:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve([FromRoute] int doctorId, CancellationToken ct)
    {
        var (adminUserId, hospitalId) = await GetCurrentAsync(ct);
        if (adminUserId == Guid.Empty || hospitalId is null)
        {
            return Forbid();
        }

        var doctor = await _doctors.FindByIdAsync(doctorId, ct);
        if (doctor is null)
        {
            return NotFound();
        }

        if (doctor.Department?.HospitalId != hospitalId.Value)
        {
            return Forbid();
        }

        if (doctor.ProfileStatus != DoctorProfileStatus.Pending)
        {
            return Conflict("Bu doktorun profili onay beklemiyor.");
        }

        doctor.ProfileStatus = DoctorProfileStatus.Approved;
        doctor.ProfileApprovedAtUtc = _clock.UtcNow;
        doctor.ProfileApprovedByUserId = adminUserId;

        await _doctors.SaveChangesAsync(ct);
        return NoContent();
    }
}
