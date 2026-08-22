namespace WebAppointmentApi.Application.Sma.Dtos;

public sealed record SmaCaseAdminDto(
    int Id,
    string Slug,
    string DisplayName,
    string ProvinceSlug,
    string ProvinceName,
    string? Story,
    string Iban,
    string BankAccountHolderName,
    string? PhotoUrl,
    bool IsVerified,
    bool IsPublished,
    DateTimeOffset CreatedAtUtc
);
