using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Controllers;

/// <summary>
/// Proxies the WhatsApp bridge's connection status/QR code so the admin panel can show a
/// "scan to connect" screen instead of requiring someone to watch the bridge's terminal.
/// </summary>
[ApiController]
[Route("api/admin/whatsapp")]
[Authorize(Roles = "Admin")]
public sealed class WhatsAppAdminController : ControllerBase
{
    private readonly IWhatsAppBridgeStatusClient _bridge;

    public WhatsAppAdminController(IWhatsAppBridgeStatusClient bridge)
    {
        _bridge = bridge;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
        => Ok(new { status = await _bridge.GetStatusAsync(ct) });

    [HttpGet("qr")]
    public async Task<IActionResult> Qr(CancellationToken ct)
        => Ok(new { qr = await _bridge.GetQrAsync(ct) });
}
