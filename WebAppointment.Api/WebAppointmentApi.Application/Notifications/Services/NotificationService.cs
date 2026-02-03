using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Notifications.Abstractions;
using WebAppointmentApi.Application.Notifications.Dtos;

namespace WebAppointmentApi.Application.Notifications.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;

    public NotificationService(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    public Task<IReadOnlyList<NotificationDto>> GetMyAsync(Guid userId, int take, CancellationToken ct)
        => _notifications.ListByUserIdAsync(userId, take, ct);

    public Task<int> DispatchUnsentAsync(CancellationToken ct)
        => _notifications.MarkUnsentAsSentAsync(ct);

    public Task<int> MarkAllReadAsync(Guid userId, CancellationToken ct)
        => _notifications.MarkAllReadAsync(userId, ct);

    public Task<int> MarkReadAsync(Guid userId, long notificationId, CancellationToken ct)
        => _notifications.MarkReadAsync(userId, notificationId, ct);
}
