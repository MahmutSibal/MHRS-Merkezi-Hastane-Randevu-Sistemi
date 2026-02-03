using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Notifications.Dtos;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<NotificationDto>> ListByUserIdAsync(Guid userId, int take, CancellationToken ct)
    {
        if (take <= 0) take = 10;
        if (take > 50) take = 50;

        return await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(take)
            .Select(n => new NotificationDto(
                n.Id,
                n.AppointmentId,
                n.Message,
                n.CreatedAtUtc,
                n.IsSent))
            .ToListAsync(ct);
    }

    public async Task<int> MarkUnsentAsSentAsync(CancellationToken ct)
    {
        var unsent = await _db.Notifications
            .Where(n => !n.IsSent)
            .ToListAsync(ct);

        foreach (var notification in unsent)
        {
            notification.IsSent = true;
        }

        if (unsent.Count == 0)
        {
            return 0;
        }

        await _db.SaveChangesAsync(ct);
        return unsent.Count;
    }
}
