namespace WebAppointmentApi.Application.Sma.Dtos;

public sealed record SmaCaseDto(
    string Slug,
    string DisplayName,
    string ProvinceSlug,
    string ProvinceName,
    string? Story,
    string Iban,
    string BankAccountHolderName,
    string? PhotoUrl
);
