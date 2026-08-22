using WebAppointmentApi.Application.Sma.Dtos;

namespace WebAppointmentApi.Application.Sma.Abstractions;

public interface ISmaCaseService
{
    Task<SiteSettingsDto> GetSiteSettingsAsync(CancellationToken ct);
    Task SetSmaEnabledAsync(bool isEnabled, CancellationToken ct);

    Task<IReadOnlyList<SmaCaseDto>> ListPublishedAsync(string? provinceSlug, CancellationToken ct);
    Task<SmaCaseDto> GetBySlugAsync(string slug, CancellationToken ct);

    Task<IReadOnlyList<SmaCaseAdminDto>> ListAllAsync(CancellationToken ct);
    Task<SmaCaseAdminDto> CreateAsync(CreateSmaCaseRequest request, CancellationToken ct);
    Task<SmaCaseAdminDto> UpdateAsync(int id, UpdateSmaCaseRequest request, CancellationToken ct);
    Task SetStatusAsync(int id, bool isVerified, bool isPublished, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
