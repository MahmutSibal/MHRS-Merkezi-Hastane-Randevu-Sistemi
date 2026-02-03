using System.Data;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Appointments.State;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorRepository _doctors;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;
    private readonly IAppointmentStateMachine _stateMachine;
    private readonly IUnitOfWork _uow;

    public AppointmentService(
        IAppointmentRepository appointments,
        IDoctorRepository doctors,
        IUserRepository users,
        IDateTimeProvider clock,
        IAppointmentStateMachine stateMachine,
        IUnitOfWork uow)
    {
        _appointments = appointments;
        _doctors = doctors;
        _users = users;
        _clock = clock;
        _stateMachine = stateMachine;
        _uow = uow;
    }

    public async Task<AppointmentDto> CreateAsync(Guid userId, CreateAppointmentRequest request, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("Unauthenticated.");
        }

        var userExists = await _users.ExistsByIdAsync(userId, ct);
        if (!userExists)
        {
            // Token might be valid but references a user that no longer exists (e.g. DB recreated).
            throw new UnauthorizedException("Invalid session. Please login again.");
        }

        var doctor = await _doctors.FindByIdAsync(request.DoctorId, ct);
        if (doctor is null)
        {
            throw new NotFoundException("Doctor not found.");
        }

        if (!doctor.IsActive)
        {
            throw new ConflictException("Doctor is not active.");
        }

        var startAtUtc = request.AppointmentDate.ToUniversalTime();
        if (startAtUtc <= _clock.UtcNow)
        {
            throw new ConflictException("Appointment start time must be in the future.");
        }

        // Fixed duration: 30 minutes
        var endAtUtc = startAtUtc.AddMinutes(30);

        await _uow.BeginAsync(ct, IsolationLevel.Serializable);
        try
        {
            var id = Guid.NewGuid();
            var created = await _appointments.CreateWithLockAsync(id, userId, request.DoctorId, startAtUtc, endAtUtc, ct);
            await _uow.CommitAsync(ct);

            return new AppointmentDto(
                Id: created.Id,
                UserId: created.UserId,
                DoctorId: created.DoctorId,
                DoctorName: doctor.Name,
                DepartmentName: doctor.Department?.Name ?? string.Empty,
                AppointmentDateUtc: created.StartAt.ToUniversalTime(),
                Status: created.Status.ToString());
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already booked", StringComparison.OrdinalIgnoreCase))
        {
            await _uow.RollbackAsync(ct);
            throw new ConflictException(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already has", StringComparison.OrdinalIgnoreCase))
        {
            await _uow.RollbackAsync(ct);
            throw new ConflictException(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            await _uow.RollbackAsync(ct);
            throw new NotFoundException(ex.Message);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetMyAsync(Guid userId, CancellationToken ct)
    {
        return await _appointments.ListMyDtosAsync(userId, ct);
    }

    public async Task<IReadOnlyList<AdminAppointmentDto>> GetAdminAllAsync(CancellationToken ct)
    {
        return await _appointments.ListAdminDtosAsync(null, ct);
    }

    public async Task<IReadOnlyList<AdminAppointmentDto>> GetAdminAsync(AppointmentListFilter filter, CancellationToken ct)
    {
        return await _appointments.ListAdminDtosAsync(filter, ct);
    }

    public async Task CancelAsync(Guid userId, Guid appointmentId, CancellationToken ct)
    {
        var appt = await _appointments.FindByIdAsync(appointmentId, ct);
        if (appt is null)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (appt.UserId != userId)
        {
            throw new ForbiddenException("Cannot cancel another user's appointment.");
        }

        _stateMachine.Transition(appt, AppointmentStatus.Cancelled, _clock.UtcNow);

        await _appointments.SaveChangesAsync(ct);
    }
}
