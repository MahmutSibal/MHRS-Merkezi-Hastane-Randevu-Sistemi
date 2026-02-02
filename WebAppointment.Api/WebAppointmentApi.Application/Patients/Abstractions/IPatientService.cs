using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.Application.Patients.Abstractions;

public interface IPatientService
{
    Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken ct);
    Task<IReadOnlyList<PatientDto>> ListAsync(CancellationToken ct);
    Task<PatientDto> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
