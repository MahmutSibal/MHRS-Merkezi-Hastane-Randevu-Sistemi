using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospital/appointments")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;
    private readonly IUserRepository _users;

    public HospitalAppointmentsController(IAppointmentService appointments, IUserRepository users)
    {
        _appointments = appointments;
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminAppointmentDto>>> GetAll(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] string? status,
        [FromQuery] int? skip,
        [FromQuery] int? take,
        CancellationToken ct)
    {
        if (!TryParseStatus(status, out var parsedStatus))
        {
            return BadRequest("Geçersiz randevu durumu.");
        }

        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null)
        {
            return Ok(Array.Empty<AdminAppointmentDto>());
        }

        var filter = new AppointmentListFilter(fromUtc, toUtc, parsedStatus, hospitalId, skip, take);
        var items = await _appointments.GetAdminAsync(filter, ct);
        return Ok(items);
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

    private static bool TryParseStatus(string? status, out AppointmentStatus? parsed)
    {
        parsed = null;
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        if (Enum.TryParse<AppointmentStatus>(status, true, out var value))
        {
            parsed = value;
            return true;
        }

        return false;
    }
}
