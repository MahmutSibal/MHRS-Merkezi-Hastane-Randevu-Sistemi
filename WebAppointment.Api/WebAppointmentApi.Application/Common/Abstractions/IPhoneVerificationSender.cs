namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IPhoneVerificationSender
{
    Task SendCodeAsync(string phone, string code, CancellationToken ct);
}
