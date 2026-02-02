using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Controllers;

/// <summary>
/// Patient appointment endpoints.
/// </summary>
[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "Patient")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;
    private readonly IUserContext _user;

    /// <summary>
    /// Creates a new instance of <see cref="AppointmentsController"/>.
    /// </summary>
    public AppointmentsController(IAppointmentService appointments, IUserContext user)
    {
        _appointments = appointments;
        _user = user;
    }

    /// <summary>
    /// Creates a new appointment for the currently authenticated patient.
    /// </summary>
    /// <remarks>
    /// - Appointment duration is fixed (30 minutes).
    /// - Overlapping appointments are prevented for the same Doctor and the same Patient.
    /// - Concurrency-safe: transactional creation with SQL-level locks.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var dto = await _appointments.CreateAsync(_user.UserId, request, ct);
        return Created($"/api/appointments/{dto.Id}", dto);
    }

    /// <summary>
    /// Returns appointments for the currently authenticated patient.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<AppointmentDto>> My(CancellationToken ct)
        => _appointments.GetMyAsync(_user.UserId, ct);

    /// <summary>
    /// Cancels an appointment owned by the currently authenticated patient.
    /// </summary>
    /// <remarks>
    /// Business rules (enforced server-side):
    /// - Past appointments cannot be cancelled.
    /// - Appointment cannot be cancelled within 15 minutes of the start time.
    /// </remarks>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken ct)
    {
        await _appointments.CancelAsync(_user.UserId, id, ct);
        return NoContent();
    }
}
