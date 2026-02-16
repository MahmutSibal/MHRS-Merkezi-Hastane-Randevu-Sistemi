namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IWhatsAppMessageSender
{
    Task SendMessageAsync(string phone, string message, CancellationToken ct);
}