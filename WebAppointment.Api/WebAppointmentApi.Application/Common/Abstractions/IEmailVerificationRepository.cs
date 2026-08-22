using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IEmailVerificationRepository
{
    Task<EmailVerificationCode?> FindLatestByUserIdAsync(Guid userId, CancellationToken ct);
    Task AddAsync(EmailVerificationCode code, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
