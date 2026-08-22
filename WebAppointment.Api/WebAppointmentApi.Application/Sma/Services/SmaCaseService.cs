using System.Globalization;
using System.Text;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Sma.Abstractions;
using WebAppointmentApi.Application.Sma.Dtos;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Application.Sma.Services;

public sealed class SmaCaseService : ISmaCaseService
{
    private readonly ISmaCaseRepository _cases;
    private readonly ISiteSettingsRepository _settings;
    private readonly IDateTimeProvider _clock;
    private readonly ITenantContext _tenant;

    public SmaCaseService(
        ISmaCaseRepository cases, ISiteSettingsRepository settings, IDateTimeProvider clock, ITenantContext tenant)
    {
        _cases = cases;
        _settings = settings;
        _clock = clock;
        _tenant = tenant;
    }

    public async Task<SiteSettingsDto> GetSiteSettingsAsync(CancellationToken ct)
    {
        var settings = await _settings.GetOrCreateAsync(ct);
        return new SiteSettingsDto(settings.IsSmaEnabled);
    }

    public async Task SetSmaEnabledAsync(bool isEnabled, CancellationToken ct)
    {
        var settings = await _settings.GetOrCreateAsync(ct);
        settings.IsSmaEnabled = isEnabled;
        await _settings.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SmaCaseDto>> ListPublishedAsync(string? provinceSlug, CancellationToken ct)
    {
        if (!await IsSmaEnabledAsync(ct))
        {
            return Array.Empty<SmaCaseDto>();
        }

        var list = await _cases.ListPublishedAsync(provinceSlug, ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<SmaCaseDto> GetBySlugAsync(string slug, CancellationToken ct)
    {
        if (!await IsSmaEnabledAsync(ct))
        {
            throw new NotFoundException("Kayıt bulunamadı.");
        }

        var entity = await _cases.FindBySlugAsync(slug, ct);
        if (entity is null || !entity.IsVerified || !entity.IsPublished)
        {
            throw new NotFoundException("Kayıt bulunamadı.");
        }

        return ToDto(entity);
    }

    private async Task<bool> IsSmaEnabledAsync(CancellationToken ct)
        => (await _settings.GetOrCreateAsync(ct)).IsSmaEnabled;

    public async Task<IReadOnlyList<SmaCaseAdminDto>> ListAllAsync(CancellationToken ct)
    {
        var list = await _cases.ListAllAsync(ct);
        return list.Select(ToAdminDto).ToList();
    }

    public async Task<SmaCaseAdminDto> CreateAsync(CreateSmaCaseRequest request, CancellationToken ct)
    {
        ValidateRequired(request.DisplayName, request.ProvinceSlug, request.ProvinceName, request.Iban, request.BankAccountHolderName);

        var now = _clock.UtcNow;
        var entity = new SmaCase
        {
            Slug = await GenerateUniqueSlugAsync(request.DisplayName, ct),
            DisplayName = request.DisplayName.Trim(),
            ProvinceSlug = request.ProvinceSlug.Trim(),
            ProvinceName = request.ProvinceName.Trim(),
            Story = string.IsNullOrWhiteSpace(request.Story) ? null : request.Story.Trim(),
            Iban = NormalizeIban(request.Iban),
            BankAccountHolderName = request.BankAccountHolderName.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim(),
            IsVerified = false,
            IsPublished = false,
            CreatedAtUtc = now,
            TenantId = _tenant.TenantId,
        };

        await _cases.AddAsync(entity, ct);
        await _cases.SaveChangesAsync(ct);

        return ToAdminDto(entity);
    }

    public async Task<SmaCaseAdminDto> UpdateAsync(int id, UpdateSmaCaseRequest request, CancellationToken ct)
    {
        ValidateRequired(request.DisplayName, request.ProvinceSlug, request.ProvinceName, request.Iban, request.BankAccountHolderName);

        var entity = await _cases.FindByIdAsync(id, ct);
        if (entity is null)
        {
            throw new NotFoundException("Kayıt bulunamadı.");
        }

        entity.DisplayName = request.DisplayName.Trim();
        entity.ProvinceSlug = request.ProvinceSlug.Trim();
        entity.ProvinceName = request.ProvinceName.Trim();
        entity.Story = string.IsNullOrWhiteSpace(request.Story) ? null : request.Story.Trim();
        entity.Iban = NormalizeIban(request.Iban);
        entity.BankAccountHolderName = request.BankAccountHolderName.Trim();
        entity.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl.Trim();
        entity.UpdatedAtUtc = _clock.UtcNow;

        await _cases.SaveChangesAsync(ct);

        return ToAdminDto(entity);
    }

    public async Task SetStatusAsync(int id, bool isVerified, bool isPublished, CancellationToken ct)
    {
        var entity = await _cases.FindByIdAsync(id, ct);
        if (entity is null)
        {
            throw new NotFoundException("Kayıt bulunamadı.");
        }

        entity.IsVerified = isVerified;
        entity.IsPublished = isPublished;
        entity.UpdatedAtUtc = _clock.UtcNow;

        await _cases.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _cases.FindByIdAsync(id, ct);
        if (entity is null)
        {
            throw new NotFoundException("Kayıt bulunamadı.");
        }

        await _cases.DeleteAsync(entity, ct);
        await _cases.SaveChangesAsync(ct);
    }

    private static void ValidateRequired(string displayName, string provinceSlug, string provinceName, string iban, string accountHolder)
    {
        if (string.IsNullOrWhiteSpace(displayName)
            || string.IsNullOrWhiteSpace(provinceSlug)
            || string.IsNullOrWhiteSpace(provinceName)
            || string.IsNullOrWhiteSpace(iban)
            || string.IsNullOrWhiteSpace(accountHolder))
        {
            throw new ConflictException("Ad, il, IBAN ve hesap sahibi adı zorunludur.");
        }
    }

    private static string NormalizeIban(string iban)
        => new string(iban.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

    private async Task<string> GenerateUniqueSlugAsync(string displayName, CancellationToken ct)
    {
        var baseSlug = Slugify(displayName);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "vaka";
        }

        var slug = baseSlug;
        var suffix = 2;
        while (await _cases.SlugExistsAsync(slug, ct))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var map = new Dictionary<char, char>
        {
            ['ç'] = 'c', ['Ç'] = 'c', ['ğ'] = 'g', ['Ğ'] = 'g', ['ı'] = 'i', ['I'] = 'i',
            ['İ'] = 'i', ['ö'] = 'o', ['Ö'] = 'o', ['ş'] = 's', ['Ş'] = 's', ['ü'] = 'u', ['Ü'] = 'u',
        };

        var chars = value.Trim().Select(c => map.TryGetValue(c, out var mapped) ? mapped : char.ToLowerInvariant(c));
        var normalized = new string(chars.ToArray()).Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(normalized.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());

        var slugChars = withoutDiacritics.Select(c => char.IsLetterOrDigit(c) ? c : '-');
        var raw = new string(slugChars.ToArray());

        while (raw.Contains("--", StringComparison.Ordinal))
        {
            raw = raw.Replace("--", "-", StringComparison.Ordinal);
        }

        return raw.Trim('-');
    }

    private static SmaCaseDto ToDto(SmaCase x) => new(
        x.Slug, x.DisplayName, x.ProvinceSlug, x.ProvinceName, x.Story, x.Iban, x.BankAccountHolderName, x.PhotoUrl);

    private static SmaCaseAdminDto ToAdminDto(SmaCase x) => new(
        x.Id, x.Slug, x.DisplayName, x.ProvinceSlug, x.ProvinceName, x.Story, x.Iban, x.BankAccountHolderName,
        x.PhotoUrl, x.IsVerified, x.IsPublished, x.CreatedAtUtc);
}
