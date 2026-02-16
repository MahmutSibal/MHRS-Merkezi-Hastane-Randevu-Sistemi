using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IPhoneVerificationRepository
{
    Task<PhoneVerificationCode?> FindLatestByPhoneAsync(string phone, CancellationToken ct);
    Task AddAsync(PhoneVerificationCode code, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
