using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Security;

/// <summary>
/// NVI (Nüfus ve Vatandaşlık İşleri) TC Kimlik No doğrulama servisi.
/// SOAP API: https://tckimlik.nvi.gov.tr/Service/KPSPublicV2.asmx
/// </summary>
public sealed class NviKimlikService : INviKimlikService
{
    private const string ServiceUrl = "https://tckimlik.nvi.gov.tr/Service/KPSPublicV2.asmx";
    private const string SoapAction = "http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula";

    private readonly HttpClient _httpClient;
    private readonly ILogger<NviKimlikService> _logger;

    public NviKimlikService(HttpClient httpClient, ILogger<NviKimlikService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateAsync(long tcKimlikNo, string firstName, string lastName, DateOnly birthDate, CancellationToken ct = default)
    {
        // NVI servisi Türkçe büyük harf bekler
        var upperFirstName = firstName.Trim().ToUpper(CultureInfo.GetCultureInfo("tr-TR"));
        var upperLastName = lastName.Trim().ToUpper(CultureInfo.GetCultureInfo("tr-TR"));

        var soapEnvelope = string.Format(
            CultureInfo.InvariantCulture,
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                 xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                 xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <TCKimlikNoDogrula xmlns=""http://tckimlik.nvi.gov.tr/WS"">
      <TCKimlikNo>{0}</TCKimlikNo>
      <Ad>{1}</Ad>
      <Soyad>{2}</Soyad>
      <DogumGun>{3}</DogumGun>
      <DogumAy>{4}</DogumAy>
      <DogumYil>{5}</DogumYil>
    </TCKimlikNoDogrula>
  </soap12:Body>
</soap12:Envelope>",
            tcKimlikNo,
            SecurityElement(upperFirstName),
            SecurityElement(upperLastName),
            birthDate.Day,
            birthDate.Month,
            birthDate.Year);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ServiceUrl);
            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
            request.Headers.TryAddWithoutValidation("SOAPAction", SoapAction);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync(ct);

            var doc = XDocument.Parse(xml);
            XNamespace ws = "http://tckimlik.nvi.gov.tr/WS";
            var result = doc.Descendants(ws + "TCKimlikNoDogrulaResult").FirstOrDefault();

            if (result is null)
            {
                _logger.LogWarning("NVI servisi beklenmeyen yanıt döndü. TC={TC}", tcKimlikNo);
                return false;
            }

            return bool.TryParse(result.Value, out var valid) && valid;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "NVI servisine bağlanılamadı. TC={TC}", tcKimlikNo);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NVI doğrulama hatası. TC={TC}", tcKimlikNo);
            return false;
        }
    }

    /// <summary>
    /// XML injection'ı önlemek için özel karakterleri escape eder.
    /// </summary>
    private static string SecurityElement(string value)
    {
        return System.Security.SecurityElement.Escape(value) ?? value;
    }
}
