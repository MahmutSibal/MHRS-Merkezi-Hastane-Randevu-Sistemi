using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/doctor/time-offs")]
[Authorize(Roles = "Doctor")]
[Authorize(Policy = "DoctorProfile")]
public sealed class DoctorTimeOffsController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly IDoctorRepository _doctors;
    private readonly IDoctorTimeOffRepository _timeOffs;
    private readonly IDateTimeProvider _clock;
    private readonly ITenantContext _tenant;

    public DoctorTimeOffsController(
        IUserContext user,
        IDoctorRepository doctors,
        IDoctorTimeOffRepository timeOffs,
        IDateTimeProvider clock,
        ITenantContext tenant)
    {
        _user = user;
        _doctors = doctors;
        _timeOffs = timeOffs;
        _clock = clock;
        _tenant = tenant;
    }

    public sealed record DoctorTimeOffDto(long Id, DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, string? Reason);

    public sealed record CreateMyTimeOffRequest(DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, string? Reason);

    [HttpPost("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateMy([FromBody] CreateMyTimeOffRequest request, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(_user.UserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var startUtc = request.StartAtUtc.ToUniversalTime();
        var endUtc = request.EndAtUtc.ToUniversalTime();
        if (endUtc <= startUtc)
        {
            return BadRequest("EndAtUtc, StartAtUtc'tan sonra olmalıdır.");
        }

        if (startUtc <= _clock.UtcNow)
        {
            return BadRequest("Geçmiş zaman için izin girilemez.");
        }

        await _timeOffs.AddAsync(new DoctorTimeOff
        {
            DoctorId = doctor.Id,
            StartAtUtc = startUtc,
            EndAtUtc = endUtc,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            TenantId = _tenant.TenantId,
        }, ct);

        await _timeOffs.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<DoctorTimeOffDto>>> ListMy([FromQuery] DateTimeOffset fromUtc, [FromQuery] DateTimeOffset toUtc, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(_user.UserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var from = fromUtc.ToUniversalTime();
        var to = toUtc.ToUniversalTime();
        if (to <= from)
        {
            return BadRequest("toUtc, fromUtc'tan sonra olmalıdır.");
        }

        var list = await _timeOffs.ListForDoctorBetweenAsync(doctor.Id, from, to, ct);
        return Ok(list.Select(x => new DoctorTimeOffDto(x.Id, x.StartAtUtc, x.EndAtUtc, x.Reason)).ToList());
    }
}
