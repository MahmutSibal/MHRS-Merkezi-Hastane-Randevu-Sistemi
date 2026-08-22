using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Messaging;

public sealed class WhatsAppBridgeStatusClient : IWhatsAppBridgeStatusClient
{
    private readonly HttpClient _http;
    private readonly ILogger<WhatsAppBridgeStatusClient> _logger;

    public WhatsAppBridgeStatusClient(HttpClient http, ILogger<WhatsAppBridgeStatusClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<string> GetStatusAsync(CancellationToken ct)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<StatusResponse>("status", ct);
            return result?.Status ?? "unreachable";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp bridge status check failed.");
            return "unreachable";
        }
    }

    public async Task<string?> GetQrAsync(CancellationToken ct)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<QrResponse>("qr", ct);
            return result?.Qr;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp bridge QR fetch failed.");
            return null;
        }
    }

    private sealed class StatusResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    private sealed class QrResponse
    {
        [JsonPropertyName("qr")]
        public string? Qr { get; set; }
    }
}
