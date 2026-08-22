using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Security;

public sealed class RecaptchaValidator : IRecaptchaValidator
{
    private readonly HttpClient _http;
    private readonly RecaptchaOptions _options;
    private readonly ILogger<RecaptchaValidator> _logger;

    public RecaptchaValidator(HttpClient http, IOptions<RecaptchaOptions> options, ILogger<RecaptchaValidator> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> IsValidAsync(string? token, CancellationToken ct)
    {
        if (_options.Disabled)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = _options.SecretKey,
                ["response"] = token,
            });

            using var response = await _http.PostAsync("recaptcha/api/siteverify", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<RecaptchaSiteVerifyResponse>(cancellationToken: ct);
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "reCAPTCHA dogrulama hatasi.");
            return false;
        }
    }

    private sealed class RecaptchaSiteVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
