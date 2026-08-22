using System.Net.Http.Json;
using WebAppointmentApi.Application.Common;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Messaging;

public sealed class WhatsAppMessageSender : IWhatsAppMessageSender
{
    private readonly HttpClient _http;

    public WhatsAppMessageSender(HttpClient http)
    {
        _http = http;
    }

    public async Task SendMessageAsync(string phone, string message, CancellationToken ct)
    {
        var targetPhone = PhoneNumberNormalizer.NormalizeForWhatsapp(phone);

        using var response = await _http.PostAsJsonAsync("/send-message", new
        {
            phone = targetPhone,
            message
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(text)
                ? "WhatsApp mesaj gonderimi basarisiz oldu."
                : text);
        }
    }
}