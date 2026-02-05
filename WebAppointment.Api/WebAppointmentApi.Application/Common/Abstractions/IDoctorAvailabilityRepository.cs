using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IDoctorAvailabilityRepository
{
    Task<DoctorAvailability?> FindByDoctorIdAsync(int doctorId, CancellationToken ct);

    Task UpsertAsync(DoctorAvailability availability, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
