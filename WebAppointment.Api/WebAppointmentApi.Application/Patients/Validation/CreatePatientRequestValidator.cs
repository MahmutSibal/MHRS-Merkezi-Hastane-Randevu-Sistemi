using FluentValidation;
using System.Linq;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.Application.Patients.Validation;

public sealed class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);

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
}
