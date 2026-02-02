using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/appointments")]
[Authorize(Roles = "Admin")]
public sealed class AdminAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public AdminAppointmentsController(IAppointmentService appointments)
    {
        _appointments = appointments;
    }

    [HttpGet]
    public Task<IReadOnlyList<AdminAppointmentDto>> GetAll(CancellationToken ct)
        => _appointments.GetAdminAllAsync(ct);
}
