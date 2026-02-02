using FluentValidation;
using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Validation;

public sealed class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);

        RuleFor(x => x.AppointmentDate)
            .NotEmpty()
            .Must(BeInFuture)
            .WithMessage("Randevu tarihi geçmişte olamaz")
            .Must(BeWeekday)
            .WithMessage("Hafta sonu randevu alınamaz")
            .Must(HaveValidMinutes)
            .WithMessage("Dakika değeri 00 veya 30 olmalıdır")
            .Must(BeInWorkingHours)
            .WithMessage("Randevu mesai saatleri içinde olmalıdır (09:00-17:00)");
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

    private static bool BeInWorkingHours(DateTimeOffset date)
    {
        // Use the provided offset as "local" clock for working hours.
        var time = date.TimeOfDay;
        var start = new TimeSpan(9, 0, 0);
        var lastStart = new TimeSpan(16, 30, 0);
        return time >= start && time <= lastStart;
    }
}
