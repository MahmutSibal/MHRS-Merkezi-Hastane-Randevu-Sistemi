using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IDoctorRepository
{
    Task<Doctor?> FindByIdAsync(int doctorId, CancellationToken ct);
    Task<Doctor?> FindByUserIdAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Doctor>> ListAsync(CancellationToken ct);
    Task AddAsync(Doctor doctor, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
