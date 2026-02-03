using WebAppointmentApi.Application.Notifications.Dtos;

namespace WebAppointmentApi.Application.Notifications.Abstractions;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetMyAsync(Guid userId, int take, CancellationToken ct);
    Task<int> DispatchUnsentAsync(CancellationToken ct);
    Task<int> MarkAllReadAsync(Guid userId, CancellationToken ct);
    Task<int> MarkReadAsync(Guid userId, long notificationId, CancellationToken ct);
}
