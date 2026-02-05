using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Auth.Services;

public sealed class LoginSecurityService : ILoginSecurityService
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    private const int MaxAttempts = 5;

    private readonly ILoginLockoutRepository _repo;
    private readonly IDateTimeProvider _clock;
    private readonly ITenantContext _tenant;

    public LoginSecurityService(
        ILoginLockoutRepository repo,
        IDateTimeProvider clock,
        ITenantContext tenant)
    {
        _repo = repo;
        _clock = clock;
        _tenant = tenant;
    }

    public async Task EnsureNotLockedAsync(string normalizedEmail, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var state = await _repo.FindByNormalizedEmailAsync(normalizedEmail, ct);

        if (state?.LockedUntilUtc is { } until && until > now)
        {
            throw new TooManyRequestsException("Çok fazla hatalı giriş denemesi. Lütfen 15 dakika sonra tekrar deneyin.");
        }
    }

    public async Task RegisterFailureAsync(string normalizedEmail, string? ipAddress, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var state = await _repo.FindByNormalizedEmailAsync(normalizedEmail, ct);

        if (state is null)
        {
            state = new LoginLockout
            {
                TenantId = _tenant.TenantId,
                NormalizedEmail = normalizedEmail,
                FailedCount = 1,
                FirstFailedAtUtc = now,
                LastFailedAtUtc = now,
                LockedUntilUtc = null,
                LastIpAddress = ipAddress,
            };

            await _repo.AddAsync(state, ct);
            await _repo.SaveChangesAsync(ct);
            return;
        }

        // Reset the window if it expired or lock already passed.
        var windowExpired = now - state.FirstFailedAtUtc > Window;
        var lockExpired = state.LockedUntilUtc is { } until && until <= now;

        if (windowExpired || lockExpired)
        {
            state.FailedCount = 1;
            state.FirstFailedAtUtc = now;
            state.LastFailedAtUtc = now;
            state.LockedUntilUtc = null;
            state.LastIpAddress = ipAddress;

            await _repo.SaveChangesAsync(ct);
            return;
        }

        state.FailedCount += 1;
        state.LastFailedAtUtc = now;
        state.LastIpAddress = ipAddress;

        if (state.FailedCount >= MaxAttempts)
        {
            state.LockedUntilUtc = now.Add(Window);
        }

        await _repo.SaveChangesAsync(ct);
    }

    public async Task RegisterSuccessAsync(string normalizedEmail, CancellationToken ct)
    {
        var state = await _repo.FindByNormalizedEmailAsync(normalizedEmail, ct);
        if (state is null)
        {
            return;
        }

        _repo.Remove(state);
        await _repo.SaveChangesAsync(ct);
    }
}
