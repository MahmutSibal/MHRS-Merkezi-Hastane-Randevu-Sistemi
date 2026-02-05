using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Abstractions;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(Guid userId, CreateAppointmentRequest request, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> GetMyAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<AdminAppointmentDto>> GetAdminAllAsync(CancellationToken ct);
    Task CancelAsync(Guid userId, Guid appointmentId, CancelAppointmentRequest request, CancellationToken ct);
}
