using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Abstractions;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(Guid userId, CreateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentDto> RescheduleAsync(Guid userId, Guid appointmentId, RescheduleAppointmentRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> GetMyAsync(Guid userId, CancellationToken ct);
    Task<AppointmentDto> GetMyByIdAsync(Guid userId, Guid appointmentId, CancellationToken ct);
    Task<IReadOnlyList<AdminAppointmentDto>> GetAdminAllAsync(CancellationToken ct);
    Task CancelAsync(Guid userId, Guid appointmentId, CancelAppointmentRequest request, CancellationToken ct);

    /// <summary>
    /// Cancels an appointment without checking caller ownership. Used for system-initiated
    /// cancellations (WhatsApp reply, unconfirmed-reminder auto-cancel) where there is no
    /// authenticated patient session to check against.
    /// </summary>
    Task ForceCancelAsync(Guid appointmentId, string reason, CancellationToken ct);
}
