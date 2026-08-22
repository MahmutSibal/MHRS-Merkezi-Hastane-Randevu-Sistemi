using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly AppDbContext _db;

    public EmailVerificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<EmailVerificationCode?> FindLatestByUserIdAsync(Guid userId, CancellationToken ct)
        => _db.EmailVerificationCodes
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task AddAsync(EmailVerificationCode code, CancellationToken ct)
    {
        _db.EmailVerificationCodes.Add(code);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
