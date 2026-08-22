namespace WebAppointmentApi.Application.Sma.Dtos;

public sealed record UpdateSmaCaseRequest(
    string DisplayName,
    string ProvinceSlug,
    string ProvinceName,
    string? Story,
    string Iban,
    string BankAccountHolderName,
    string? PhotoUrl
);
