using WebAppointmentApi.Application.Hospitals.Dtos;

namespace WebAppointmentApi.Application.Hospitals.Abstractions;

public interface IHospitalService
{
    Task<IReadOnlyList<HospitalDto>> ListAsync(CancellationToken ct);
    Task<IReadOnlyList<HospitalDto>> ListNearestAsync(double latitude, double longitude, int? take, CancellationToken ct);
    Task<HospitalDto> CreateAsync(CreateHospitalRequest request, CancellationToken ct);
    Task<Guid> AssignSubAdminAsync(int hospitalId, string email, string password, CancellationToken ct);
}
