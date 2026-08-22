using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Sma.Abstractions;
using WebAppointmentApi.Application.Sma.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

/// <summary>
/// Public read-only endpoints for the SMA donation directory. No payment is ever processed
/// here — this only surfaces each verified case's story and IBAN for direct bank transfer.
/// </summary>
[ApiController]
[Route("api/sma")]
[AllowAnonymous]
public sealed class SmaController : ControllerBase
{
    private readonly ISmaCaseService _cases;

    public SmaController(ISmaCaseService cases)
    {
        _cases = cases;
    }

    [HttpGet("settings")]
    public Task<SiteSettingsDto> GetSettings(CancellationToken ct)
        => _cases.GetSiteSettingsAsync(ct);

    [HttpGet("cases")]
    public Task<IReadOnlyList<SmaCaseDto>> List([FromQuery] string? province, CancellationToken ct)
        => _cases.ListPublishedAsync(province, ct);

    [HttpGet("cases/{slug}")]
    public Task<SmaCaseDto> GetBySlug([FromRoute] string slug, CancellationToken ct)
        => _cases.GetBySlugAsync(slug, ct);
}
