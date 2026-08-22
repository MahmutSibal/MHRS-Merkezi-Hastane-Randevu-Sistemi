using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Messaging;

public sealed class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly BrevoOptions _options;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(HttpClient http, Microsoft.Extensions.Options.IOptions<BrevoOptions> options, ILogger<BrevoEmailSender> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync("smtp/email", new
        {
            sender = new { name = _options.SenderName, email = _options.SenderEmail },
            to = new[] { new { email = toEmail } },
            subject,
            htmlContent = htmlBody,
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Brevo email send failed. Status={Status} Body={Body}", response.StatusCode, text);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(text)
                ? "E-posta gonderimi basarisiz oldu."
                : text);
        }
    }
}
