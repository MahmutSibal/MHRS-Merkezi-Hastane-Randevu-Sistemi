using FluentValidation;
using WebAppointmentApi.Application.Auth.Dtos;

namespace WebAppointmentApi.Application.Auth.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // En az biri dolu olmalı: Email veya TcKimlikNo
        RuleFor(x => new { x.Email, x.TcKimlikNo })
            .Must(v => !string.IsNullOrWhiteSpace(v.Email) || !string.IsNullOrWhiteSpace(v.TcKimlikNo))
            .WithMessage("E-posta veya TC Kimlik No zorunlu.");

        // Email sağlandıysa formatını doğrula
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress()
                .MaximumLength(320);
        });

        // TC sağlandıysa 11 haneli numerik olmalı
        When(x => !string.IsNullOrWhiteSpace(x.TcKimlikNo), () =>
        {
            RuleFor(x => x.TcKimlikNo!)
                .Length(11)
                .Matches("^\\d{11}$");
        });

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);
    }
}
