using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IPatientRepository
{
    Task<Patient?> FindByTcAsync(string tcKimlikNo, CancellationToken ct);
    Task<Patient?> FindByUserIdAsync(Guid userId, CancellationToken ct);
    Task<Patient?> FindByIdAsync(int id, CancellationToken ct);
    Task<Patient?> FindByPhoneSuffixAsync(string last10Digits, CancellationToken ct);
    Task<IReadOnlyList<Patient>> ListAsync(CancellationToken ct);
    Task AddAsync(Patient patient, CancellationToken ct);
    Task SoftDeleteAsync(Patient patient, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
