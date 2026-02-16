using FluentValidation;
using System.Linq;
using WebAppointmentApi.Application.Auth.Dtos;

namespace WebAppointmentApi.Application.Auth.Validation;

public sealed class PhoneVerificationRequestValidator : AbstractValidator<PhoneVerificationRequest>
{
    public PhoneVerificationRequestValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30)
            .Must(BeValidPhone)
            .WithMessage("Telefon numarasi gecersiz.");
    }

    private static bool BeValidPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length is 10 or 11;
    }
}
