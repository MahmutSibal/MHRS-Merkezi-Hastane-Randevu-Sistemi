using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/doctor/calendar")]
[Authorize(Roles = "Doctor")]
[Authorize(Policy = "DoctorProfile")]
public sealed class DoctorCalendarController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly IDoctorCalendarService _calendar;

    public DoctorCalendarController(IUserContext user, IDoctorCalendarService calendar)
    {
        _user = user;
        _calendar = calendar;
    }

    [HttpGet("daily-slots")]
    public async Task<ActionResult<IReadOnlyList<DoctorDailySlotDto>>> DailySlots([FromQuery] string date, CancellationToken ct)
    {
        if (!DateOnly.TryParse(date, out var dateOnly))
        {
            return BadRequest("Invalid date. Expected format: YYYY-MM-DD");
        }

        var slots = await _calendar.GetMyDailySlotsAsync(_user.UserId, dateOnly, ct);
        return Ok(slots);
    }
}
