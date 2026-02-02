using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Abstractions;

public interface IDoctorAppointmentService
{
    Task<IReadOnlyList<DoctorAppointmentDto>> GetMyAsync(Guid doctorUserId, CancellationToken ct);
    Task ApproveAsync(Guid doctorUserId, Guid appointmentId, CancellationToken ct);
    Task CompleteAsync(Guid doctorUserId, Guid appointmentId, CancellationToken ct);
}
