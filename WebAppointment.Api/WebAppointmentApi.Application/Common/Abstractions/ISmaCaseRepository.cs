using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface ISmaCaseRepository
{
    Task<IReadOnlyList<SmaCase>> ListPublishedAsync(string? provinceSlug, CancellationToken ct);
    Task<IReadOnlyList<SmaCase>> ListAllAsync(CancellationToken ct);
    Task<SmaCase?> FindBySlugAsync(string slug, CancellationToken ct);
    Task<SmaCase?> FindByIdAsync(int id, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);
    Task AddAsync(SmaCase smaCase, CancellationToken ct);
    Task DeleteAsync(SmaCase smaCase, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
