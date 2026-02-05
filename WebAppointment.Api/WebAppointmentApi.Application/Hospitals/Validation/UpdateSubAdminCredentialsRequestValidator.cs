using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using WebAppointmentApi.Application.Hospitals.Dtos;

namespace WebAppointmentApi.Application.Hospitals.Validation;

public sealed class UpdateSubAdminCredentialsRequestValidator : AbstractValidator<UpdateSubAdminCredentialsRequest>
{
    public UpdateSubAdminCredentialsRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .Must(IsValidEmail)
                .WithMessage("Geçerli bir e-posta girin (RFC uyumlu, Türkçe karakter içeremez).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Must(p => !Regex.IsMatch(p!, @"^\d+$")).WithMessage("Şifre sadece rakamlardan oluşamaz.");
        });
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
