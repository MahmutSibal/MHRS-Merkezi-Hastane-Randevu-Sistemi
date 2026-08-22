namespace WebAppointmentApi.Infrastructure.Security;

public sealed class RecaptchaOptions
{
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Dev/test escape hatch — when true, all tokens pass without calling Google.</summary>
    public bool Disabled { get; set; }
}
