namespace WebAppointmentApi.Application.Common;

public static class PhoneNumberNormalizer
{
    public static string NormalizeForWhatsapp(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.Length == 11 && digits.StartsWith("0", StringComparison.Ordinal))
        {
            digits = digits[1..];
        }

        if (digits.Length == 10)
        {
            return "90" + digits;
        }

        return digits;
    }

    /// <summary>Last 10 digits (national significant number, no country code) for loose DB matching.</summary>
    public static string Last10Digits(string phone)
    {
        var normalized = NormalizeForWhatsapp(phone);
        return normalized.Length >= 10 ? normalized[^10..] : normalized;
    }
}
