using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Dependents.Abstractions;
using WebAppointmentApi.Application.Dependents.Dtos;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace WebAppointmentApi.Application.Dependents.Services;

public sealed class DependentService : IDependentService
{
    private static readonly Regex FullNameRegex = new(
        @"^[\p{L}]+(?:[ '\-][\p{L}]+)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    private readonly IDependentRepository _dependents;
    private readonly IUserRepository _users;
    private readonly IPatientRepository _patients;
    private readonly IDateTimeProvider _clock;
    private readonly ITenantContext _tenant;
    private readonly INviKimlikService _nvi;
    private readonly ILogger<DependentService> _logger;

    public DependentService(
        IDependentRepository dependents,
        IUserRepository users,
        IPatientRepository patients,
        IDateTimeProvider clock,
        ITenantContext tenant,
        INviKimlikService nvi,
        ILogger<DependentService> logger)
    {
        _dependents = dependents;
        _users = users;
        _patients = patients;
        _clock = clock;
        _tenant = tenant;
        _nvi = nvi;
        _logger = logger;
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
            .Select(x => new DependentDto(x.Id, x.FullName, x.TcKimlikNo, x.BirthDate, x.Relation))
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

        if (!IsValidFullName(fullName))
        {
            throw new ConflictException("Yakın adı soyadı geçersiz. En az 2 kelime ve sadece harf içermelidir.");
        }

        if (!IsValidTckn(tckn))
        {
            throw new ConflictException("Geçersiz TCKN.");
        }

        // Guardian hasta bilgisi (TCKN ve soyad kontrolü için)
        var patient = await _patients.FindByUserIdAsync(userId, ct);
        if (patient is null)
        {
            throw new ConflictException("Hasta profili bulunamadı.");
        }

        if (string.Equals(patient.TcKimlikNo, tckn, StringComparison.Ordinal))
        {
            throw new ConflictException("Kendi TCKN'nizi yakın olarak ekleyemezsiniz.");
        }

        var dependentSurname = ExtractSurname(fullName);
        var patientSurname = (patient.LastName ?? string.Empty).Trim();
        if (!SurnamesMatch(patientSurname, dependentSurname))
        {
            throw new ConflictException("Yakın soyadı, hasta soyadı ile aynı olmalıdır.");
        }

        // Doğum tarihi zorunlu ve gelecekte olamaz.
        if (request.BirthDate == default)
        {
            throw new ConflictException("Doğum tarihi zorunludur.");
        }
        if (request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ConflictException("Doğum tarihi gelecekte olamaz.");
        }

        var existing = await _dependents.ListByGuardianUserIdAsync(userId, ct);
        if (existing.Any(x => x.TcKimlikNo == tckn))
        {
            throw new ConflictException("Bu TCKN zaten eklenmiş.");
        }

        // NVI TC Kimlik No doğrulama (hasta kayıt ile aynı sistem)
        try
        {
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var nviFirstName = string.Join(" ", nameParts.Take(nameParts.Length - 1));
            var nviLastName = nameParts[^1];

            var isValid = await _nvi.ValidateAsync(
                long.Parse(tckn, CultureInfo.InvariantCulture),
                nviFirstName,
                nviLastName,
                request.BirthDate,
                ct);

            if (!isValid)
            {
                throw new ConflictException("NVI kimlik doğrulama başarısız. Lütfen bilgileri kontrol edin.");
            }
        }
        catch (ConflictException) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NVI doğrulama servisi yakın ekleme sırasında erişilemedi. TCKN={Tckn}", tckn);
            // Graceful degradation: NVI erişilemezse kayıt devam eder
        }

        var entity = new Dependent
        {
            GuardianUserId = userId,
            FullName = fullName,
            TcKimlikNo = tckn,
            BirthDate = request.BirthDate,
            Relation = request.Relation,
            CreatedAtUtc = _clock.UtcNow,
            TenantId = _tenant.TenantId,
        };

        await _dependents.AddAsync(entity, ct);
        await _dependents.SaveChangesAsync(ct);

        return new DependentDto(entity.Id, entity.FullName, entity.TcKimlikNo, entity.BirthDate, entity.Relation);
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

    private static bool IsValidFullName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var trimmed = value.Trim();
        if (trimmed.Length < 3) return false;
        // En az iki kelime: regex bunu garanti eder.
        return FullNameRegex.IsMatch(trimmed);
    }

    private static string ExtractSurname(string fullName)
    {
        var parts = (fullName ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 0 ? string.Empty : parts[^1];
    }

    private static bool SurnamesMatch(string patientSurname, string dependentSurname)
    {
        if (string.IsNullOrWhiteSpace(patientSurname) || string.IsNullOrWhiteSpace(dependentSurname)) return false;

        var a = patientSurname.Trim().ToUpper(TrCulture);
        var b = dependentSurname.Trim().ToUpper(TrCulture);
        return string.Equals(a, b, StringComparison.Ordinal);
    }
}
