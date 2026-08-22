using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class SiteSettingsRepository : ISiteSettingsRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public SiteSettingsRepository(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<SiteSetting> GetOrCreateAsync(CancellationToken ct)
    {
        var existing = await _db.SiteSettings.SingleOrDefaultAsync(ct);
        if (existing is not null)
        {
            return existing;
        }

        var created = new SiteSetting
        {
            IsSmaEnabled = true,
            TenantId = _tenant.TenantId,
        };

        _db.SiteSettings.Add(created);
        await _db.SaveChangesAsync(ct);
        return created;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
