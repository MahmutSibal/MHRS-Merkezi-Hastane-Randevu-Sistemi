using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospital/reports")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly IUserRepository _users;

    public HospitalReportsController(IReportService reports, IUserRepository users)
    {
        _reports = reports;
        _users = users;
    }

    [HttpGet("top-doctors")]
    public Task<IReadOnlyList<TopDoctorDto>> TopDoctors([FromQuery] int days = 30, [FromQuery] int take = 10, CancellationToken ct = default)
        => _reports.GetTopDoctorsLastDaysAsync(days, take, ct);

    [HttpGet("appointment-summary")]
    public Task<AppointmentSummaryDto> AppointmentSummary([FromQuery] int days = 30, CancellationToken ct = default)
        => _reports.GetAppointmentSummaryAsync(days, ct);

    [HttpGet("no-show-risk")]
    public async Task<IReadOnlyList<NoShowRiskAppointmentDto>> NoShowRisk(
        [FromQuery] int days = 7, [FromQuery] int minScore = 40, CancellationToken ct = default)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null) return Array.Empty<NoShowRiskAppointmentDto>();
        return await _reports.GetNoShowRiskAppointmentsAsync(days, minScore, hospitalId.Value, ct);
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
}
