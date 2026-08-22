using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface ISiteSettingsRepository
{
    /// <summary>Returns the current tenant's settings row, creating a default one if missing.</summary>
    Task<SiteSetting> GetOrCreateAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
