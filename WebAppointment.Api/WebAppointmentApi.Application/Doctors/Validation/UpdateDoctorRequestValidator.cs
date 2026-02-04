using FluentValidation;
using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.Application.Doctors.Validation;

public sealed class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.DepartmentId).GreaterThan(0);
    }
}
