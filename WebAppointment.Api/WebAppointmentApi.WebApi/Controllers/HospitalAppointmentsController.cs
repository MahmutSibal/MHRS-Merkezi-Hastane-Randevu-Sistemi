using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospital/appointments")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public HospitalAppointmentsController(IAppointmentService appointments)
    {
        _appointments = appointments;
    }

    [HttpGet]
    public Task<IReadOnlyList<AdminAppointmentDto>> GetAll(CancellationToken ct)
        => _appointments.GetAdminAllAsync(ct);
}
