using FluentValidation;
using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Validation;

public sealed class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);

        RuleFor(x => x.DependentId)
            .Must(x => x is null || x.Value > 0)
            .WithMessage("DependentId geçersiz.");

        RuleFor(x => x.AppointmentDate)
            .NotEmpty()
            .Must(BeInFuture)
            .WithMessage("Randevu tarihi geçmişte olamaz")
            .Must(BeWithinOneYear)
            .WithMessage("Randevu tarihi en fazla 1 yıl içinde olmalıdır")
            .Must(BeWeekday)
            .WithMessage("Hafta sonu randevu alınamaz.")
            .Must(HaveValidMinutes)
            .WithMessage("Dakika değeri 00 veya 30 olmalıdır");
    }

    private static bool BeInFuture(DateTimeOffset date)
        => date.ToUniversalTime() > DateTimeOffset.UtcNow;

    private static bool BeWeekday(DateTimeOffset date)
    {
        var day = date.DayOfWeek;
        return day is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
    }

    private static bool HaveValidMinutes(DateTimeOffset date)
        => date.Minute is 0 or 30;

    private static bool BeWithinOneYear(DateTimeOffset date)
        => date.ToUniversalTime() <= DateTimeOffset.UtcNow.AddYears(1);
}
