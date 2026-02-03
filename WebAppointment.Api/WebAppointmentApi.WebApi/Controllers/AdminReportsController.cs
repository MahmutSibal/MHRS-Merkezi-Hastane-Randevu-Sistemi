using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public sealed class AdminReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly IBackgroundJobQueue _jobs;
    private readonly ILogger<AdminReportsController> _logger;

    public AdminReportsController(IReportService reports, IBackgroundJobQueue jobs, ILogger<AdminReportsController> logger)
    {
        _reports = reports;
        _jobs = jobs;
        _logger = logger;
    }

    [HttpGet("top-doctors")]
    public Task<IReadOnlyList<TopDoctorDto>> TopDoctors([FromQuery] int days = 30, [FromQuery] int take = 10, CancellationToken ct = default)
        => _reports.GetTopDoctorsLastDaysAsync(days, take, null, ct);

    [HttpGet("appointment-summary")]
    public Task<AppointmentSummaryDto> AppointmentSummary([FromQuery] int days = 30, CancellationToken ct = default)
        => _reports.GetAppointmentSummaryAsync(days, null, ct);

    [HttpPost("refresh-cache")]
    public IActionResult RefreshCache([FromQuery] int days = 30, [FromQuery] int take = 10)
    {
        var jobId = Guid.NewGuid();
        _jobs.Enqueue(async (sp, ct) =>
        {
            var reports = sp.GetRequiredService<IReportService>();
            await reports.GetTopDoctorsLastDaysAsync(days, take, null, ct);
            await reports.GetAppointmentSummaryAsync(days, null, ct);
            _logger.LogInformation("Report refresh job {JobId} completed.", jobId);
        });

        return Accepted(new { jobId });
    }
}
