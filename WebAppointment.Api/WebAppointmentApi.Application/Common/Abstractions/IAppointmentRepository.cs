using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IAppointmentRepository
{
    Task<Appointment> CreateWithLockAsync(
        Guid appointmentId,
        Guid userId,
        int doctorId,
        int? dependentId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        DateTimeOffset dayStartUtc,
        DateTimeOffset dayEndUtc,
        DateTimeOffset nowUtc,
        CancellationToken ct);

    Task<IReadOnlyList<Appointment>> ListByUserIdAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> ListByDoctorIdAsync(int doctorId, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> ListByDoctorIdBetweenAsync(int doctorId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct);

    // Case 6: DTO-only listing with projection (no entity return from service layer)
    Task<IReadOnlyList<AppointmentDto>> ListMyDtosAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<AdminAppointmentDto>> ListAdminDtosAsync(CancellationToken ct);

    Task<IReadOnlyList<Appointment>> ListAllAsync(CancellationToken ct);
    Task<Appointment?> FindByIdAsync(Guid appointmentId, CancellationToken ct);
    Task AddLogAsync(AppointmentLog log, CancellationToken ct);
    Task AddNotificationAsync(Notification notification, CancellationToken ct);
    Task RescheduleWithLockAsync(Guid appointmentId, Guid userId, int doctorId, DateTimeOffset newStartUtc, DateTimeOffset newEndUtc, DateTimeOffset newDayStartUtc, DateTimeOffset newDayEndUtc, DateTimeOffset nowUtc, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> ListUpcomingUnremindedAsync(DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct);
    Task MarkReminderSentAsync(Guid appointmentId, DateTimeOffset sentAtUtc, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
