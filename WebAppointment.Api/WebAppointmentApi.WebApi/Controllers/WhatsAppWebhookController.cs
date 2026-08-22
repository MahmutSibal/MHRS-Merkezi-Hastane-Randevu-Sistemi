using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Infrastructure.Messaging;

namespace WebAppointmentApi.WebApi.Controllers;

/// <summary>
/// Receives inbound WhatsApp messages forwarded by the wppconnect bridge so patients can
/// confirm or cancel their appointment reminder by replying. Not user-authenticated — guarded
/// by a shared secret the bridge sends in the X-Bridge-Secret header instead.
/// </summary>
[ApiController]
[Route("api/whatsapp")]
[AllowAnonymous]
public sealed class WhatsAppWebhookController : ControllerBase
{
    private readonly IWhatsAppReplyService _replyService;
    private readonly WhatsAppBridgeOptions _options;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(
        IWhatsAppReplyService replyService,
        IOptions<WhatsAppBridgeOptions> options,
        ILogger<WhatsAppWebhookController> logger)
    {
        _replyService = replyService;
        _options = options.Value;
        _logger = logger;
    }

    [HttpPost("inbound")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Inbound([FromBody] WhatsAppInboundMessage message, CancellationToken ct)
    {
        var secret = Request.Headers["X-Bridge-Secret"].FirstOrDefault();
        if (string.IsNullOrEmpty(_options.WebhookSecret) || secret != _options.WebhookSecret)
        {
            _logger.LogWarning("WhatsApp webhook reddedildi: gecersiz veya eksik X-Bridge-Secret.");
            return Unauthorized();
        }

        var phone = message.From ?? message.Phone ?? string.Empty;
        var text = message.Body ?? message.Message ?? string.Empty;

        await _replyService.HandleInboundReplyAsync(phone, text, ct);

        return NoContent();
    }
}

/// <summary>
/// Shape tolerant of the common wppconnect-server webhook fields. Verify against the real
/// bridge payload once it's live and trim to what it actually sends.
/// </summary>
public sealed class WhatsAppInboundMessage
{
    public string? From { get; set; }
    public string? Phone { get; set; }
    public string? Body { get; set; }
    public string? Message { get; set; }
}
