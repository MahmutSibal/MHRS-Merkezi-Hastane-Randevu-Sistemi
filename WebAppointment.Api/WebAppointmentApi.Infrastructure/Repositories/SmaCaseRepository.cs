using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class SmaCaseRepository : ISmaCaseRepository
{
    private readonly AppDbContext _db;

    public SmaCaseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SmaCase>> ListPublishedAsync(string? provinceSlug, CancellationToken ct)
    {
        var query = _db.SmaCases.AsNoTracking().Where(x => x.IsVerified && x.IsPublished);

        if (!string.IsNullOrWhiteSpace(provinceSlug))
        {
            query = query.Where(x => x.ProvinceSlug == provinceSlug);
        }

        return await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SmaCase>> ListAllAsync(CancellationToken ct)
        => await _db.SmaCases.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);

    public Task<SmaCase?> FindBySlugAsync(string slug, CancellationToken ct)
        => _db.SmaCases.SingleOrDefaultAsync(x => x.Slug == slug, ct);

    public Task<SmaCase?> FindByIdAsync(int id, CancellationToken ct)
        => _db.SmaCases.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct)
        => _db.SmaCases.AsNoTracking().AnyAsync(x => x.Slug == slug, ct);

    public Task AddAsync(SmaCase smaCase, CancellationToken ct)
    {
        _db.SmaCases.Add(smaCase);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SmaCase smaCase, CancellationToken ct)
    {
        _db.SmaCases.Remove(smaCase);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
