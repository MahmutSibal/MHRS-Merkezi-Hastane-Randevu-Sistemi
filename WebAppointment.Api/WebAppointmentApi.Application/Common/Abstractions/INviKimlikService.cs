namespace WebAppointmentApi.Application.Common.Abstractions;

/// <summary>
/// NVI (Nüfus ve Vatandaşlık İşleri) TC Kimlik No doğrulama servisi.
/// https://tckimlik.nvi.gov.tr/Modul/TcKimlikNoDogrula
/// </summary>
public interface INviKimlikService
{
    /// <summary>
    /// TC Kimlik No'yu NVI SOAP servisi üzerinden doğrular.
    /// </summary>
    Task<bool> ValidateAsync(long tcKimlikNo, string firstName, string lastName, DateOnly birthDate, CancellationToken ct = default);
}
