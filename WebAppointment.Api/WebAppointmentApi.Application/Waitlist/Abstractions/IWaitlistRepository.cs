using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Waitlist.Abstractions;

public interface IWaitlistRepository
{
    Task<bool> ExistsPendingAsync(Guid userId, int doctorId, DateOnly desiredDate, TimeOnly desiredTime, CancellationToken ct);

    Task<IReadOnlyList<WaitlistEntry>> ListPendingForSlotAsync(int doctorId, DateOnly desiredDate, TimeOnly desiredTime, int take, CancellationToken ct);

    Task AddAsync(WaitlistEntry entry, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
