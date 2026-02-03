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
                n.IsSent,
                n.IsRead))
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

    public async Task<int> MarkAllReadAsync(Guid userId, CancellationToken ct)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        if (unread.Count == 0)
        {
            return 0;
        }

        await _db.SaveChangesAsync(ct);
        return unread.Count;
    }

    public async Task<int> MarkReadAsync(Guid userId, long notificationId, CancellationToken ct)
    {
        var notification = await _db.Notifications
            .Where(n => n.UserId == userId && n.Id == notificationId)
            .SingleOrDefaultAsync(ct);

        if (notification is null || notification.IsRead)
        {
            return 0;
        }

        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return 1;
    }
}
