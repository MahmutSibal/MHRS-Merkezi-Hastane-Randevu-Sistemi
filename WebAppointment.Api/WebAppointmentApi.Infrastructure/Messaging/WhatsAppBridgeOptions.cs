namespace WebAppointmentApi.Infrastructure.Messaging;

public sealed class WhatsAppBridgeOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8080";

    /// <summary>
    /// Shared secret the wppconnect bridge must send in the X-Bridge-Secret header when it
    /// forwards inbound WhatsApp messages to our webhook. Empty means the webhook is disabled.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>Whether the API should spawn the Node bridge process itself on startup.</summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>Path to the mhrs-whatsapp-bot folder, relative to the WebApi content root.</summary>
    public string WorkingDirectory { get; set; } = "../../mhrs-whatsapp-bot";
}
