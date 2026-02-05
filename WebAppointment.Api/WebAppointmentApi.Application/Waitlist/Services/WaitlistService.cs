using System.Data;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Waitlist.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Waitlist.Services;

public sealed class WaitlistService : IWaitlistService
{
    private const int SlotMinutes = 30;

    private readonly IWaitlistRepository _waitlist;
    private readonly IAppointmentRepository _appointments;
    private readonly IUserRepository _users;
    private readonly IDoctorRepository _doctors;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;

    public WaitlistService(
        IWaitlistRepository waitlist,
        IAppointmentRepository appointments,
        IUserRepository users,
        IDoctorRepository doctors,
        IDateTimeProvider clock,
        IUnitOfWork uow,
        ITenantContext tenant)
    {
        _waitlist = waitlist;
        _appointments = appointments;
        _users = users;
        _doctors = doctors;
        _clock = clock;
        _uow = uow;
        _tenant = tenant;
    }

    public async Task JoinAsync(Guid userId, int doctorId, DateTimeOffset startAtUtc, CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("Unauthenticated.");
        }

        var userExists = await _users.ExistsByIdAsync(userId, ct);
        if (!userExists)
        {
            throw new UnauthorizedException("Invalid session. Please login again.");
        }

        var doctor = await _doctors.FindByIdAsync(doctorId, ct);
        if (doctor is null)
        {
            throw new NotFoundException("Doctor not found.");
        }

        if (!doctor.IsActive)
        {
            throw new ConflictException("Doctor is not active.");
        }

        var nowUtc = _clock.UtcNow;
        if (startAtUtc.ToUniversalTime() <= nowUtc)
        {
            throw new ConflictException("Start time must be in the future.");
        }

        var localStart = startAtUtc.ToUniversalTime().ToLocalTime();
        var desiredDate = DateOnly.FromDateTime(localStart.DateTime);
        var desiredTime = TimeOnly.FromDateTime(localStart.DateTime);

        var exists = await _waitlist.ExistsPendingAsync(userId, doctorId, desiredDate, desiredTime, ct);
        if (exists)
        {
            throw new ConflictException("Bu slot için zaten bekleme listesindesiniz.");
        }

        var entry = new WaitlistEntry
        {
            UserId = userId,
            DoctorId = doctorId,
            DesiredDate = desiredDate,
            DesiredTime = desiredTime,
            Status = WaitlistStatus.Pending,
            CreatedAtUtc = nowUtc,
            TenantId = _tenant.TenantId,
        };

        await _waitlist.AddAsync(entry, ct);
        await _waitlist.SaveChangesAsync(ct);
    }

    public async Task TryReserveFreedSlotAsync(int doctorId, DateTimeOffset startAtUtc, CancellationToken ct)
    {
        var nowUtc = _clock.UtcNow;

        var localStart = startAtUtc.ToUniversalTime().ToLocalTime();
        var desiredDate = DateOnly.FromDateTime(localStart.DateTime);
        var desiredTime = TimeOnly.FromDateTime(localStart.DateTime);

        var candidates = await _waitlist.ListPendingForSlotAsync(doctorId, desiredDate, desiredTime, take: 20, ct);
        if (candidates.Count == 0)
        {
            return;
        }

        foreach (var entry in candidates)
        {
            try
            {
                // Build day window in the user's local offset (same technique as AppointmentService).
                var dayStartUtc = new DateTimeOffset(localStart.Date, localStart.Offset).ToUniversalTime();
                var dayEndUtc = dayStartUtc.AddDays(1);

                var startUtc = startAtUtc.ToUniversalTime();
                var endUtc = startUtc.AddMinutes(SlotMinutes);

                await _uow.BeginAsync(ct, IsolationLevel.Serializable);

                var appointmentId = Guid.NewGuid();
                await _appointments.CreateWithLockAsync(
                    appointmentId,
                    entry.UserId,
                    doctorId,
                    dependentId: null,
                    startUtc,
                    endUtc,
                    dayStartUtc,
                    dayEndUtc,
                    nowUtc,
                    ct);

                entry.Status = WaitlistStatus.Confirmed;
                entry.NotifiedAtUtc = nowUtc;

                await _waitlist.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);
                return;
            }
            catch (InvalidOperationException)
            {
                await _uow.RollbackAsync(ct);
                // Slot could not be reserved for this user (rule conflict, etc). Try next.
                continue;
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }
    }
}
