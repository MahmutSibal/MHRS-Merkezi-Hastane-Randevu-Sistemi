namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IRecaptchaValidator
{
    /// <summary>Verifies a reCAPTCHA v2 token against Google's siteverify endpoint.</summary>
    Task<bool> IsValidAsync(string? token, CancellationToken ct);
}
