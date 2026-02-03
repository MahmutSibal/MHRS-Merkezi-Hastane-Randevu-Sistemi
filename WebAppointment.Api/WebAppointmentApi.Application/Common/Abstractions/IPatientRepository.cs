using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IPatientRepository
{
    Task<Patient?> FindByTcAsync(string tcKimlikNo, CancellationToken ct);
    Task<Patient?> FindByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Patient>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<Patient>> ListByHospitalAsync(int hospitalId, CancellationToken ct);
    Task<bool> IsPatientInHospitalAsync(int patientId, int hospitalId, CancellationToken ct);
    Task AddAsync(Patient patient, CancellationToken ct);
    Task SoftDeleteAsync(Patient patient, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
