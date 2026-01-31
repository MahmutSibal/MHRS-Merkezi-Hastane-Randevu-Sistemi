using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/doctor/appointments")]
[Authorize(Roles = "Doctor")]
[Authorize(Policy = "DoctorProfile")]
public sealed class DoctorAppointmentsController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly IDoctorAppointmentService _doctorAppointments;

    public DoctorAppointmentsController(IUserContext user, IDoctorAppointmentService doctorAppointments)
    {
        _user = user;
        _doctorAppointments = doctorAppointments;
    }

    [HttpGet("my")]
    public Task<IReadOnlyList<DoctorAppointmentDto>> My(CancellationToken ct)
        => _doctorAppointments.GetMyAsync(_user.UserId, ct);

    [HttpPut("{appointmentId:guid}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid appointmentId, CancellationToken ct)
    {
        await _doctorAppointments.ApproveAsync(_user.UserId, appointmentId, ct);
        return NoContent();
    }

    [HttpPut("{appointmentId:guid}/complete")]
    public async Task<IActionResult> Complete([FromRoute] Guid appointmentId, CancellationToken ct)
    {
        await _doctorAppointments.CompleteAsync(_user.UserId, appointmentId, ct);
        return NoContent();
    }
}
