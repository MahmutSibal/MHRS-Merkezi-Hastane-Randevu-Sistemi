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
    public async Task<IReadOnlyList<TopDoctorDto>> TopDoctors([FromQuery] int days = 30, [FromQuery] int take = 10, CancellationToken ct = default)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null)
        {
            return Array.Empty<TopDoctorDto>();
        }

        return await _reports.GetTopDoctorsLastDaysAsync(days, take, hospitalId, ct);
    }

    [HttpGet("appointment-summary")]
    public async Task<AppointmentSummaryDto> AppointmentSummary([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var hospitalId = await GetCurrentHospitalIdAsync(ct);
        if (hospitalId is null)
        {
            return new AppointmentSummaryDto(days, new AppointmentStatusSummaryDto(0, 0, 0, 0, 0), new List<DailyAppointmentCountDto>());
        }

        return await _reports.GetAppointmentSummaryAsync(days, hospitalId, ct);
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
