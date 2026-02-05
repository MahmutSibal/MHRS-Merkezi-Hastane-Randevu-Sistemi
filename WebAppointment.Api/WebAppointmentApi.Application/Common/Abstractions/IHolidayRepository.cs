using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IHolidayRepository
{
    Task<bool> ExistsAsync(DateOnly date, CancellationToken ct);

    Task<IReadOnlyList<Holiday>> ListAsync(int take, CancellationToken ct);

    Task AddAsync(Holiday holiday, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
