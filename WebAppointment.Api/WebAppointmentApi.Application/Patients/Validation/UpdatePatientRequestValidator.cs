using FluentValidation;
using System.Linq;
using System.Text.RegularExpressions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.Application.Patients.Validation;

public sealed class UpdatePatientRequestValidator : AbstractValidator<UpdatePatientRequest>
{
    private static readonly Regex NameRegex = new(
        @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public UpdatePatientRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

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
        return digits.Length is 10 or 11;
    }
}
