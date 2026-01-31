using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public sealed class AdminReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public AdminReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("top-doctors")]
    public Task<IReadOnlyList<TopDoctorDto>> TopDoctors([FromQuery] int days = 30, [FromQuery] int take = 10, CancellationToken ct = default)
        => _reports.GetTopDoctorsLastDaysAsync(days, take, ct);
}
