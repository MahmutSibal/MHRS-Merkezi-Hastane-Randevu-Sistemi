namespace WebAppointmentApi.Application.Appointments.Abstractions;

public interface IWhatsAppReplyService
{
    /// <summary>
    /// Handles an inbound WhatsApp message received via the bridge webhook — matches it to
    /// the patient's nearest unconfirmed reminded appointment and acts on confirm/cancel replies.
    /// </summary>
    Task HandleInboundReplyAsync(string rawPhone, string text, CancellationToken ct);
}
