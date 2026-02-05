using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Dependents.Abstractions;
using WebAppointmentApi.Application.Dependents.Dtos;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Dependents.Services;

public sealed class DependentService : IDependentService
{
    private readonly IDependentRepository _dependents;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;
    private readonly ITenantContext _tenant;

    public DependentService(
        IDependentRepository dependents,
        IUserRepository users,
        IDateTimeProvider clock,
        ITenantContext tenant)
    {
        _dependents = dependents;
        _users = users;
        _clock = clock;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<DependentDto>> ListMyAsync(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("Unauthenticated.");
        }

        var userExists = await _users.ExistsByIdAsync(userId, ct);
        if (!userExists)
        {
            throw new UnauthorizedException("Invalid session. Please login again.");
        }

        var list = await _dependents.ListByGuardianUserIdAsync(userId, ct);
        return list
            .OrderBy(x => x.FullName)
            .Select(x => new DependentDto(x.Id, x.FullName, x.TcKimlikNo))
            .ToList();
    }

    public async Task<DependentDto> CreateAsync(Guid userId, CreateDependentRequest request, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("Unauthenticated.");
        }

        var userExists = await _users.ExistsByIdAsync(userId, ct);
        if (!userExists)
        {
            throw new UnauthorizedException("Invalid session. Please login again.");
        }

        var fullName = (request.FullName ?? string.Empty).Trim();
        var tckn = (request.TcKimlikNo ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
        {
            throw new ConflictException("Çocuk adı soyadı zorunludur.");
        }

        if (!IsValidTckn(tckn))
        {
            throw new ConflictException("Geçersiz TCKN.");
        }

        var existing = await _dependents.ListByGuardianUserIdAsync(userId, ct);
        if (existing.Any(x => x.TcKimlikNo == tckn))
        {
            throw new ConflictException("Bu TCKN zaten eklenmiş.");
        }

        var entity = new Dependent
        {
            GuardianUserId = userId,
            FullName = fullName,
            TcKimlikNo = tckn,
            CreatedAtUtc = _clock.UtcNow,
            TenantId = _tenant.TenantId,
        };

        await _dependents.AddAsync(entity, ct);
        await _dependents.SaveChangesAsync(ct);

        return new DependentDto(entity.Id, entity.FullName, entity.TcKimlikNo);
    }

    public async Task DeleteAsync(Guid userId, int dependentId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("Unauthenticated.");
        }

        var dep = await _dependents.FindByIdAsync(dependentId, ct);
        if (dep is null)
        {
            throw new NotFoundException("Dependent not found.");
        }

        if (dep.GuardianUserId != userId)
        {
            throw new ForbiddenException("Forbidden.");
        }

        await _dependents.DeleteAsync(dep, ct);
        await _dependents.SaveChangesAsync(ct);
    }

    // Offline validation: 11 digits, not starting with 0, checksum rules.
    private static bool IsValidTckn(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length != 11) return false;
        if (value[0] == '0') return false;
        if (!value.All(char.IsDigit)) return false;

        var digits = value.Select(c => c - '0').ToArray();

        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var digit10 = ((oddSum * 7) - evenSum) % 10;
        if (digit10 < 0) digit10 += 10;
        if (digits[9] != digit10) return false;

        var sumFirst10 = digits.Take(10).Sum();
        var digit11 = sumFirst10 % 10;
        return digits[10] == digit11;
    }
}
