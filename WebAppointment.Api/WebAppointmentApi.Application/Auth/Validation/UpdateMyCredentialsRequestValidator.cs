using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using WebAppointmentApi.Application.Auth.Dtos;

namespace WebAppointmentApi.Application.Auth.Validation;

public sealed class UpdateMyCredentialsRequestValidator : AbstractValidator<UpdateMyCredentialsRequest>
{
    public UpdateMyCredentialsRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre zorunludur.");

        When(x => !string.IsNullOrWhiteSpace(x.NewEmail), () =>
        {
            RuleFor(x => x.NewEmail)
                .Must(IsValidEmail)
                .WithMessage("Geçerli bir e-posta girin (RFC uyumlu, Türkçe karakter içeremez).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.NewPassword), () =>
        {
            RuleFor(x => x.NewPassword)
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Must(p => !Regex.IsMatch(p!, @"^\d+$")).WithMessage("Şifre sadece rakamlardan oluşamaz.");
        });

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.NewEmail) || !string.IsNullOrWhiteSpace(x.NewPassword))
            .WithMessage("Yeni e-posta veya yeni şifre zorunludur.");
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var trimmed = email.Trim();
        if (trimmed.IndexOf('\u0131') >= 0 || trimmed.IndexOf('\u0130') >= 0) return false; // ı, İ
        try
        {
            var addr = new MailAddress(trimmed);
            if (!string.Equals(addr.Address, trimmed, StringComparison.OrdinalIgnoreCase)) return false;
            if (string.IsNullOrWhiteSpace(addr.Host) || !addr.Host.Contains('.')) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
