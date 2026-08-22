using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Sma.Abstractions;
using WebAppointmentApi.Application.Sma.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/sma")]
[Authorize(Roles = "Admin")]
public sealed class AdminSmaController : ControllerBase
{
    private readonly ISmaCaseService _cases;

    public AdminSmaController(ISmaCaseService cases)
    {
        _cases = cases;
    }

    [HttpGet("settings")]
    public Task<SiteSettingsDto> GetSettings(CancellationToken ct)
        => _cases.GetSiteSettingsAsync(ct);

    [HttpPatch("settings")]
    public async Task<IActionResult> SetSettings([FromBody] SetSiteSettingsRequest request, CancellationToken ct)
    {
        await _cases.SetSmaEnabledAsync(request.IsSmaEnabled, ct);
        return NoContent();
    }

    [HttpGet]
    public Task<IReadOnlyList<SmaCaseAdminDto>> List(CancellationToken ct)
        => _cases.ListAllAsync(ct);

    [HttpPost]
    public Task<SmaCaseAdminDto> Create([FromBody] CreateSmaCaseRequest request, CancellationToken ct)
        => _cases.CreateAsync(request, ct);

    [HttpPut("{id:int}")]
    public Task<SmaCaseAdminDto> Update([FromRoute] int id, [FromBody] UpdateSmaCaseRequest request, CancellationToken ct)
        => _cases.UpdateAsync(id, request, ct);

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus([FromRoute] int id, [FromBody] SetSmaCaseStatusRequest request, CancellationToken ct)
    {
        await _cases.SetStatusAsync(id, request.IsVerified, request.IsPublished, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _cases.DeleteAsync(id, ct);
        return NoContent();
    }
}

public sealed record SetSmaCaseStatusRequest(bool IsVerified, bool IsPublished);
public sealed record SetSiteSettingsRequest(bool IsSmaEnabled);
