namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct);
}
