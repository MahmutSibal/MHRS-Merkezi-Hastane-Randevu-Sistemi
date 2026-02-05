namespace WebAppointmentApi.Application.Waitlist.Abstractions;

public interface IWaitlistService
{
    Task JoinAsync(Guid userId, int doctorId, DateTimeOffset startAtUtc, CancellationToken ct);

    Task TryReserveFreedSlotAsync(int doctorId, DateTimeOffset startAtUtc, CancellationToken ct);
}
