namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IWhatsAppBridgeStatusClient
{
    /// <summary>Returns the bridge's connection status, or "unreachable" if it can't be reached.</summary>
    Task<string> GetStatusAsync(CancellationToken ct);

    /// <summary>Returns the latest QR code as a data: URI, or null if none is pending.</summary>
    Task<string?> GetQrAsync(CancellationToken ct);
}
