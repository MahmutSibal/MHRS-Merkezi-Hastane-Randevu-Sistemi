using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospital/reports")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public HospitalReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("top-doctors")]
    public Task<IReadOnlyList<TopDoctorDto>> TopDoctors([FromQuery] int days = 30, [FromQuery] int take = 10, CancellationToken ct = default)
        => _reports.GetTopDoctorsLastDaysAsync(days, take, ct);

    [HttpGet("appointment-summary")]
    public Task<AppointmentSummaryDto> AppointmentSummary([FromQuery] int days = 30, CancellationToken ct = default)
        => _reports.GetAppointmentSummaryAsync(days, ct);
}
