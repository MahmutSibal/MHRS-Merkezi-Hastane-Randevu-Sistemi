using System.Linq;
using System.Net.Http.Json;
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
        var targetPhone = NormalizeForWhatsapp(phone);

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

    private static string NormalizeForWhatsapp(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.Length == 11 && digits.StartsWith("0", StringComparison.Ordinal))
        {
            digits = digits[1..];
        }

        if (digits.Length == 10)
        {
            return "90" + digits;
        }

        return digits;
    }
}