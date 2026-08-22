namespace WebAppointmentApi.Application.Sma.Dtos;

public sealed record CreateSmaCaseRequest(
    string DisplayName,
    string ProvinceSlug,
    string ProvinceName,
    string? Story,
    string Iban,
    string BankAccountHolderName,
    string? PhotoUrl
);
