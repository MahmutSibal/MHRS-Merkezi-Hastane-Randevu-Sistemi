using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Doctors.Dtos;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/doctor/profile")]
[Authorize(Roles = "Doctor")]
[Authorize(Policy = "DoctorProfile")]
public sealed class DoctorProfileController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly IDoctorRepository _doctors;
    private readonly IDateTimeProvider _clock;

    public DoctorProfileController(IUserContext user, IDoctorRepository doctors, IDateTimeProvider clock)
    {
        _user = user;
        _doctors = doctors;
        _clock = clock;
    }

    [HttpGet("me")]
    public async Task<ActionResult<DoctorProfileDto>> GetMy(CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(_user.UserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        return Ok(new DoctorProfileDto(
            DoctorId: doctor.Id,
            GraduationUniversity: doctor.GraduationUniversity,
            ExperienceSummary: doctor.ExperienceSummary,
            ProfileStatus: doctor.ProfileStatus.ToString(),
            SubmittedAtUtc: doctor.ProfileSubmittedAtUtc,
            ApprovedAtUtc: doctor.ProfileApprovedAtUtc));
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMy([FromBody] UpdateDoctorProfileRequest request, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(_user.UserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var uni = (request.GraduationUniversity ?? string.Empty).Trim();
        var exp = (request.ExperienceSummary ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(uni) || uni.Length < 2)
        {
            return BadRequest("Mezun olunan üniversite zorunludur.");
        }
        if (string.IsNullOrWhiteSpace(exp) || exp.Length < 10)
        {
            return BadRequest("Deneyim/tecrübe alanı zorunludur (en az 10 karakter).");
        }

        doctor.GraduationUniversity = uni;
        doctor.ExperienceSummary = exp;
        doctor.ProfileStatus = DoctorProfileStatus.Pending;
        doctor.ProfileSubmittedAtUtc = _clock.UtcNow;
        doctor.ProfileApprovedAtUtc = null;
        doctor.ProfileApprovedByUserId = null;

        await _doctors.SaveChangesAsync(ct);
        return NoContent();
    }
}
