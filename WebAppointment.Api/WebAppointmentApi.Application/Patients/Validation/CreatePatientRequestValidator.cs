using FluentValidation;
using System.Linq;
using System.Text.RegularExpressions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.Application.Patients.Validation;

public sealed class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    private static readonly Regex NameRegex = new(
        @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public CreatePatientRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress()
                .MaximumLength(320);
        });

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidName)
            .WithMessage("Ad geçersiz.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidName)
            .WithMessage("Soyad geçersiz.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30)
            .Must(BeValidPhone)
            .WithMessage("Telefon numarası geçersiz.");

        RuleFor(x => x.TcKimlikNo)
            .NotEmpty()
            .Length(11)
            .Must(BeValidTcKimlikNo)
            .WithMessage("Geçersiz TC Kimlik No");
    }

    private static bool BeValidTcKimlikNo(string tc)
    {
        if (string.IsNullOrWhiteSpace(tc)) return false;
        if (tc.Length != 11) return false;
        if (tc[0] == '0') return false;
        if (!tc.All(char.IsDigit)) return false;

        var digits = tc.Select(c => c - '0').ToArray();

        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];

        var digit10 = ((oddSum * 7) - evenSum) % 10;
        if (digit10 < 0) digit10 += 10;

        if (digits[9] != digit10) return false;

        var sumFirst10 = digits.Take(10).Sum();
        var digit11 = sumFirst10 % 10;

        return digits[10] == digit11;
    }

    private static bool BeValidName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var trimmed = value.Trim();
        if (trimmed.Length < 2) return false;
        return NameRegex.IsMatch(trimmed);
    }

    private static bool BeValidPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var digits = new string(value.Where(char.IsDigit).ToArray());

        // TR kullanımında sık görülen formatlar:
        // - 10 hane (5551234567)
        // - 11 hane (05551234567)
        if (digits.Length is 10) return true;
        if (digits.Length is 11) return true;

        return false;
    }
}
