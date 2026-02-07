using FluentValidation;
using System.Linq;
using System.Text.RegularExpressions;
using WebAppointmentApi.Application.Dependents.Dtos;

namespace WebAppointmentApi.Application.Dependents.Validation;

public sealed class CreateDependentRequestValidator : AbstractValidator<CreateDependentRequest>
{
    private static readonly Regex FullNameRegex = new(
        @"^[\p{L}]+(?:[ '\-][\p{L}]+)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public CreateDependentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200)
            .Must(x => FullNameRegex.IsMatch(x.Trim()))
            .WithMessage("Yakın adı soyadı geçersiz.");

        RuleFor(x => x.TcKimlikNo)
            .NotEmpty()
            .Length(11)
            .Must(x => x.All(char.IsDigit))
            .WithMessage("Geçersiz TCKN.");

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Doğum tarihi zorunludur.");

        RuleFor(x => x.Relation)
            .IsInEnum()
            .WithMessage("Yakınlık türü geçersiz.");
    }
}
