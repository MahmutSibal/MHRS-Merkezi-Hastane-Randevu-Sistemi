using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/doctor/availability")]
[Authorize(Roles = "Doctor")]
[Authorize(Policy = "DoctorProfile")]
public sealed class DoctorAvailabilityController : ControllerBase
{
    private static readonly TimeOnly DefaultWorkStart = new(9, 0);
    private static readonly TimeOnly DefaultWorkEnd = new(17, 0);

    private readonly IUserContext _user;
    private readonly IDoctorRepository _doctors;
    private readonly IDoctorAvailabilityRepository _availability;
    private readonly ITenantContext _tenant;

    public DoctorAvailabilityController(
        IUserContext user,
        IDoctorRepository doctors,
        IDoctorAvailabilityRepository availability,
        ITenantContext tenant)
    {
        _user = user;
        _doctors = doctors;
        _availability = availability;
        _tenant = tenant;
    }

    public sealed record DoctorAvailabilityDto(
        string WorkStart,
        string WorkEnd,
        string? LunchStart,
        string? LunchEnd,
        int SlotMinutes);

    public sealed record UpdateMyAvailabilityRequest(
        string WorkStart,
        string WorkEnd,
        string? LunchStart,
        string? LunchEnd,
        int SlotMinutes);

    [HttpGet("me")]
    public async Task<ActionResult<DoctorAvailabilityDto>> GetMy(CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(_user.UserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var a = await _availability.FindByDoctorIdAsync(doctor.Id, ct);

        return Ok(new DoctorAvailabilityDto(
            WorkStart: (a?.WorkStart ?? DefaultWorkStart).ToString("HH:mm"),
            WorkEnd: (a?.WorkEnd ?? DefaultWorkEnd).ToString("HH:mm"),
            LunchStart: a?.LunchStart?.ToString("HH:mm"),
            LunchEnd: a?.LunchEnd?.ToString("HH:mm"),
            SlotMinutes: a?.SlotMinutes is > 0 ? a!.SlotMinutes : 30));
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMy([FromBody] UpdateMyAvailabilityRequest request, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(_user.UserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        if (!TimeOnly.TryParse(request.WorkStart, out var ws) || !TimeOnly.TryParse(request.WorkEnd, out var we))
        {
            return BadRequest("Geçersiz saat formatı. Örn: 09:00");
        }

        if (we <= ws)
        {
            return BadRequest("WorkEnd, WorkStart'tan sonra olmalıdır.");
        }

        TimeOnly? ls = null;
        TimeOnly? le = null;
        if (!string.IsNullOrWhiteSpace(request.LunchStart) || !string.IsNullOrWhiteSpace(request.LunchEnd))
        {
            if (!TimeOnly.TryParse(request.LunchStart, out var parsedLs) || !TimeOnly.TryParse(request.LunchEnd, out var parsedLe))
            {
                return BadRequest("Geçersiz öğle arası saat formatı. Örn: 12:00");
            }
            if (parsedLe <= parsedLs)
            {
                return BadRequest("LunchEnd, LunchStart'tan sonra olmalıdır.");
            }
            ls = parsedLs;
            le = parsedLe;
        }

        var slot = request.SlotMinutes;
        if (slot != 30)
        {
            return BadRequest("Şu an sadece 30 dakikalık slot destekleniyor (SlotMinutes=30).");
        }

        var entity = new DoctorAvailability
        {
            DoctorId = doctor.Id,
            WorkStart = ws,
            WorkEnd = we,
            LunchStart = ls,
            LunchEnd = le,
            SlotMinutes = slot,
            TenantId = _tenant.TenantId,
        };

        await _availability.UpsertAsync(entity, ct);
        await _availability.SaveChangesAsync(ct);

        return NoContent();
    }
}
