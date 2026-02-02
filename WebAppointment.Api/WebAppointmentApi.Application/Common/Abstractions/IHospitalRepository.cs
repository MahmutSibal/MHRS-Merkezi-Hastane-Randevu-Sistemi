using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IHospitalRepository
{
    Task<Hospital?> FindByIdAsync(int hospitalId, CancellationToken ct);
    Task<IReadOnlyList<Hospital>> ListAsync(CancellationToken ct);
    Task AddAsync(Hospital hospital, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
