using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.Application.Doctors.Abstractions;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorDto>> ListAsync(CancellationToken ct);
    Task<DoctorDto> CreateAsync(CreateDoctorRequest request, CancellationToken ct);
    Task<DoctorDto> UpdateAsync(int doctorId, UpdateDoctorRequest request, CancellationToken ct);
    Task DeleteAsync(int doctorId, CancellationToken ct);
}
