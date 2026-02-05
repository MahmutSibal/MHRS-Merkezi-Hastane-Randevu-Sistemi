using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/holidays")]
[Authorize(Roles = "Admin")]
public sealed class AdminHolidaysController : ControllerBase
{
    private readonly IHolidayRepository _holidays;
    private readonly ITenantContext _tenant;

    public AdminHolidaysController(IHolidayRepository holidays, ITenantContext tenant)
    {
        _holidays = holidays;
        _tenant = tenant;
    }

    public sealed record HolidayDto(int Id, string Date, string Name);

    public sealed record CreateHolidayRequest(string Date, string Name);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HolidayDto>>> List([FromQuery] int take = 50, CancellationToken ct = default)
    {
        var items = await _holidays.ListAsync(Math.Clamp(take, 1, 200), ct);
        return Ok(items.Select(x => new HolidayDto(x.Id, x.Date.ToString("yyyy-MM-dd"), x.Name)).ToList());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create([FromBody] CreateHolidayRequest request, CancellationToken ct)
    {
        if (!DateOnly.TryParse(request.Date, out var date))
        {
            return BadRequest("Geçersiz tarih. Format: YYYY-MM-DD");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Tatil adı zorunludur.");
        }

        var exists = await _holidays.ExistsAsync(date, ct);
        if (exists)
        {
            return Conflict("Bu tarih için zaten tatil tanımlı.");
        }

        await _holidays.AddAsync(new Holiday
        {
            Date = date,
            Name = request.Name.Trim(),
            TenantId = _tenant.TenantId,
        }, ct);

        await _holidays.SaveChangesAsync(ct);
        return NoContent();
    }
}
