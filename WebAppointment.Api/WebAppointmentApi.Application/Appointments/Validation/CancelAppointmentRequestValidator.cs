using FluentValidation;
using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Validation;

public sealed class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("İptal gerekçesi en fazla 500 karakter olabilir.");
    }
}
