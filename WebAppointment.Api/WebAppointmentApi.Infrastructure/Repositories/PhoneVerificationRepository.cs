using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class PhoneVerificationRepository : IPhoneVerificationRepository
{
    private readonly AppDbContext _db;

    public PhoneVerificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<PhoneVerificationCode?> FindLatestByPhoneAsync(string phone, CancellationToken ct)
        => _db.PhoneVerificationCodes
            .Where(x => x.Phone == phone)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task AddAsync(PhoneVerificationCode code, CancellationToken ct)
    {
        _db.PhoneVerificationCodes.Add(code);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
