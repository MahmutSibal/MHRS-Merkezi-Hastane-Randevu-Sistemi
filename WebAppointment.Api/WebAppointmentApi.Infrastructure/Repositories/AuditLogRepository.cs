using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditLog>> ListAsync(string? entity, string? action, int take, CancellationToken ct)
    {
        take = take <= 0 ? 50 : Math.Min(take, 500);

        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entity))
        {
            var e = entity.Trim();
            query = query.Where(x => x.Entity == e);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            var a = action.Trim();
            query = query.Where(x => x.Action == a);
        }

        return await query
            .OrderByDescending(x => x.TimestampUtc)
            .Take(take)
            .ToListAsync(ct);
    }
}
