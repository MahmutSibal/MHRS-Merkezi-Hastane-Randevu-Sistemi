using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

/// <summary>
/// A published donation-directory listing for an SMA patient: shows their story and IBAN so
/// donors can transfer directly through their own bank. This system never holds or moves money.
/// </summary>
public sealed class SmaCase : IMultiTenant
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public string ProvinceSlug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;

    public string? Story { get; set; }
    public string Iban { get; set; } = string.Empty;
    public string BankAccountHolderName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public bool IsVerified { get; set; }
    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public int TenantId { get; set; }
}
