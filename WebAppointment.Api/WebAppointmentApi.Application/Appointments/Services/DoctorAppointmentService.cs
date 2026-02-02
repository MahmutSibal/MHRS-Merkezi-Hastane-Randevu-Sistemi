using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Appointments.State;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class DoctorAppointmentService : IDoctorAppointmentService
{
    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorRepository _doctors;
    private readonly IDateTimeProvider _clock;
    private readonly IAppointmentStateMachine _stateMachine;

    public DoctorAppointmentService(
        IAppointmentRepository appointments,
        IDoctorRepository doctors,
        IDateTimeProvider clock,
        IAppointmentStateMachine stateMachine)
    {
        _appointments = appointments;
        _doctors = doctors;
        _clock = clock;
        _stateMachine = stateMachine;
    }

    public async Task<IReadOnlyList<DoctorAppointmentDto>> GetMyAsync(Guid doctorUserId, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(doctorUserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var list = await _appointments.ListByDoctorIdAsync(doctor.Id, ct);

        return list.Select(x => new DoctorAppointmentDto(
                Id: x.Id,
                PatientUserId: x.UserId,
                PatientEmail: x.User?.Email ?? string.Empty,
                DoctorId: x.DoctorId,
                StartAtUtc: x.StartAt.ToUniversalTime(),
                EndAtUtc: x.EndAt.ToUniversalTime(),
                Status: x.Status.ToString()))
            .ToList();
    }

    public async Task ApproveAsync(Guid doctorUserId, Guid appointmentId, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(doctorUserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var appt = await _appointments.FindByIdAsync(appointmentId, ct);
        if (appt is null)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (appt.DoctorId != doctor.Id)
        {
            throw new ForbiddenException("Cannot manage another doctor's appointment.");
        }

        _stateMachine.Transition(appt, AppointmentStatus.Approved, _clock.UtcNow);

        await _appointments.AddLogAsync(new AppointmentLog
        {
            AppointmentId = appt.Id,
            Action = "Approved",
            CreatedAtUtc = _clock.UtcNow,
        }, ct);

        await _appointments.AddNotificationAsync(new Notification
        {
            AppointmentId = appt.Id,
            UserId = appt.UserId,
            Message = "Randevunuz onaylandı.",
            CreatedAtUtc = _clock.UtcNow,
            IsSent = false,
        }, ct);

        await _appointments.SaveChangesAsync(ct);
    }

    public async Task CompleteAsync(Guid doctorUserId, Guid appointmentId, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(doctorUserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        var appt = await _appointments.FindByIdAsync(appointmentId, ct);
        if (appt is null)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (appt.DoctorId != doctor.Id)
        {
            throw new ForbiddenException("Cannot manage another doctor's appointment.");
        }

        _stateMachine.Transition(appt, AppointmentStatus.Completed, _clock.UtcNow);

        await _appointments.AddLogAsync(new AppointmentLog
        {
            AppointmentId = appt.Id,
            Action = "Completed",
            CreatedAtUtc = _clock.UtcNow,
        }, ct);

        await _appointments.AddNotificationAsync(new Notification
        {
            AppointmentId = appt.Id,
            UserId = appt.UserId,
            Message = "Randevunuz tamamlandı.",
            CreatedAtUtc = _clock.UtcNow,
            IsSent = false,
        }, ct);

        await _appointments.SaveChangesAsync(ct);
    }
}
