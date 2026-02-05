using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IDoctorTimeOffRepository
{
    Task<IReadOnlyList<DoctorTimeOff>> ListForDoctorBetweenAsync(int doctorId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct);

    Task AddAsync(DoctorTimeOff timeOff, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
