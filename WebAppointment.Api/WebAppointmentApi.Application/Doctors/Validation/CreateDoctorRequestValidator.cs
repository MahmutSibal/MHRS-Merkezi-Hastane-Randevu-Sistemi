using System.Net.Mail;
using System.Text.RegularExpressions;
using FluentValidation;
using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.Application.Doctors.Validation;

public sealed class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.DepartmentId).GreaterThan(0);

        // If either Email or Password is provided, both must be valid
        When(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .Must(IsValidEmail).WithMessage("Geçerli bir e-posta girin (RFC uyumlu, Türkçe karakter içeremez).");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Must(p => !Regex.IsMatch(p, @"^\d+$")).WithMessage("Şifre sadece rakamlardan oluşamaz.");
        });
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var trimmed = email.Trim();
        // Disallow Turkish-specific letters
        if (trimmed.IndexOf('\u0131') >= 0 || trimmed.IndexOf('\u0130') >= 0) return false; // ı, İ
        try
        {
            var addr = new MailAddress(trimmed);
            // Ensure normalized address matches to avoid invalid unicode tricks
            if (!string.Equals(addr.Address, trimmed, StringComparison.OrdinalIgnoreCase)) return false;
            // Basic domain check
            if (string.IsNullOrWhiteSpace(addr.Host) || !addr.Host.Contains('.')) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
