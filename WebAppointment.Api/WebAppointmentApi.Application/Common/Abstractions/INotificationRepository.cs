using WebAppointmentApi.Application.Notifications.Dtos;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface INotificationRepository
{
    Task<IReadOnlyList<NotificationDto>> ListByUserIdAsync(Guid userId, int take, CancellationToken ct);
    Task<int> MarkUnsentAsSentAsync(CancellationToken ct);
}
