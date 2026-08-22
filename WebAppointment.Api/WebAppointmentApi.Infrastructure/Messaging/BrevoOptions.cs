namespace WebAppointmentApi.Infrastructure.Messaging;

public sealed class BrevoOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "MHRS";
}
