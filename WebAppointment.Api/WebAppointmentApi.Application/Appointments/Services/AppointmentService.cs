using System.Data;
using System.Globalization;
using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Appointments.State;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Waitlist.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorRepository _doctors;
    private readonly IDoctorAvailabilityRepository _availability;
    private readonly IDoctorTimeOffRepository _timeOffs;
    private readonly IHolidayRepository _holidays;
    private readonly IDependentRepository _dependents;
    private readonly IPatientRepository _patients;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;
    private readonly IAppointmentStateMachine _stateMachine;
    private readonly IUnitOfWork _uow;
    private readonly IWaitlistService _waitlist;
    private readonly IWhatsAppMessageSender _whatsappSender;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointments,
        IDoctorRepository doctors,
        IDoctorAvailabilityRepository availability,
        IDoctorTimeOffRepository timeOffs,
        IHolidayRepository holidays,
        IDependentRepository dependents,
        IPatientRepository patients,
        IUserRepository users,
        IDateTimeProvider clock,
        IAppointmentStateMachine stateMachine,
        IUnitOfWork uow,
        IWaitlistService waitlist,
        IWhatsAppMessageSender whatsappSender,
        ILogger<AppointmentService> logger)
    {
        _appointments = appointments;
        _doctors = doctors;
        _availability = availability;
        _timeOffs = timeOffs;
        _holidays = holidays;
        _dependents = dependents;
        _patients = patients;
        _users = users;
        _clock = clock;
        _stateMachine = stateMachine;
        _uow = uow;
        _waitlist = waitlist;
        _whatsappSender = whatsappSender;
        _logger = logger;
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

        Dependent? dependent = null;
        if (request.DependentId is not null)
        {
            dependent = await _dependents.FindByIdAsync(request.DependentId.Value, ct);
            if (dependent is null)
            {
                throw new NotFoundException("Dependent not found.");
            }
            if (dependent.GuardianUserId != userId)
            {
                throw new ForbiddenException("Forbidden.");
            }
        }

        var nowUtc = _clock.UtcNow;

        var startAtUtc = request.AppointmentDate.ToUniversalTime();
        if (startAtUtc <= nowUtc)
        {
            throw new ConflictException("Appointment start time must be in the future.");
        }

        // En fazla 1 yıl sonrasına randevu alınabilir.
        var maxAllowedUtc = nowUtc.AddYears(1);
        if (startAtUtc > maxAllowedUtc)
        {
            throw new ConflictException("En fazla 1 yıl içinde randevu alınabilir.");
        }

        // Fixed duration: 30 minutes
        var endAtUtc = startAtUtc.AddMinutes(30);

        // Enforce working rules (availability/lunch/time off/holiday) based on the user's provided offset.
        var localDate = DateOnly.FromDateTime(request.AppointmentDate.DateTime);
        var localStart = TimeOnly.FromDateTime(request.AppointmentDate.DateTime);
        var localEnd = localStart.AddMinutes(30);

        if (localDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            throw new ConflictException("Hafta sonu randevu alınamaz.");
        }

        if (localStart.Minute % 30 != 0)
        {
            throw new ConflictException("Appointment start time must be aligned to 30-minute slots.");
        }

        if (await _holidays.ExistsAsync(localDate, ct))
        {
            throw new ConflictException("Appointments cannot be created on holidays.");
        }

        var availability = await _availability.FindByDoctorIdAsync(doctor.Id, ct);
        var workStart = availability?.WorkStart ?? new TimeOnly(9, 0);
        var workEnd = availability?.WorkEnd ?? new TimeOnly(17, 0);

        if (localStart < workStart || localEnd > workEnd)
        {
            throw new ConflictException("Appointment time is outside doctor's working hours.");
        }

        if (availability?.LunchStart is not null && availability.LunchEnd is not null)
        {
            var ls = availability.LunchStart.Value;
            var le = availability.LunchEnd.Value;
            var overlapsLunch = localStart < le && localEnd > ls;
            if (overlapsLunch)
            {
                throw new ConflictException("Appointment time overlaps doctor's lunch break.");
            }
        }

        // Gün sınırlarını, kullanıcının gönderdiği saat dilimi offset'ine göre hesapla.
        // Böylece "1 günde 1 randevu" kuralı, kullanıcının seçtiği güne göre uygulanır.
        var dayStartUtc = new DateTimeOffset(request.AppointmentDate.Date, request.AppointmentDate.Offset).ToUniversalTime();
        var dayEndUtc = dayStartUtc.AddDays(1);

        var timeOffs = await _timeOffs.ListForDoctorBetweenAsync(doctor.Id, dayStartUtc, dayEndUtc, ct);
        var overlapsTimeOff = timeOffs.Any(t => startAtUtc < t.EndAtUtc && endAtUtc > t.StartAtUtc);
        if (overlapsTimeOff)
        {
            throw new ConflictException("Appointment time overlaps doctor's time off.");
        }

        await _uow.BeginAsync(ct, IsolationLevel.Serializable);
        try
        {
            var id = Guid.NewGuid();
            var created = await _appointments.CreateWithLockAsync(
                id,
                userId,
                request.DoctorId,
                dependent?.Id,
                startAtUtc,
                endAtUtc,
                dayStartUtc,
                dayEndUtc,
                nowUtc,
                ct);
            await _uow.CommitAsync(ct);

            await TrySendAppointmentWhatsAppAsync(userId, request, doctor, ct);

            return new AppointmentDto(
                Id: created.Id,
                UserId: created.UserId,
                DoctorId: created.DoctorId,
                DoctorName: doctor.Name,
                DepartmentName: doctor.Department?.Name ?? string.Empty,
                AppointmentDateUtc: created.StartAt.ToUniversalTime(),
                Status: created.Status.ToString(),
                DependentId: dependent?.Id,
                DependentFullName: dependent?.FullName);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            await _uow.RollbackAsync(ct);
            throw new NotFoundException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await _uow.RollbackAsync(ct);
            throw new ConflictException(ex.Message);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    private async Task TrySendAppointmentWhatsAppAsync(Guid userId, CreateAppointmentRequest request, Doctor doctor, CancellationToken ct)
    {
        try
        {
            var patient = await _patients.FindByUserIdAsync(userId, ct);
            if (patient is null || string.IsNullOrWhiteSpace(patient.Phone))
            {
                _logger.LogWarning("Appointment WhatsApp skipped: patient phone missing. UserId={UserId}", userId);
                return;
            }

            var culture = CultureInfo.GetCultureInfo("tr-TR");
            var localDate = request.AppointmentDate.ToString("dd MMMM yyyy HH:mm", culture);
            var hospitalName = doctor.Department?.Hospital?.Name ?? string.Empty;
            var departmentName = doctor.Department?.Name ?? string.Empty;
            var doctorName = doctor.Name;

            var message = string.Format(culture,
                "Randevunuz olusturuldu. Hastane: {0}, Departman: {1}, Doktor: {2}, Tarih: {3}.",
                hospitalName,
                departmentName,
                doctorName,
                localDate);

            await _whatsappSender.SendMessageAsync(patient.Phone, message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment WhatsApp message. UserId={UserId} DoctorId={DoctorId}", userId, doctor.Id);
        }
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetMyAsync(Guid userId, CancellationToken ct)
    {
        return await _appointments.ListMyDtosAsync(userId, ct);
    }

    public async Task<AppointmentDto> GetMyByIdAsync(Guid userId, Guid appointmentId, CancellationToken ct)
    {
        var appt = await _appointments.FindByIdAsync(appointmentId, ct);
        if (appt is null)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (appt.UserId != userId)
        {
            throw new ForbiddenException("Forbidden.");
        }

        var doctor = await _doctors.FindByIdAsync(appt.DoctorId, ct);
        if (doctor is null)
        {
            throw new NotFoundException("Doctor not found.");
        }

        Dependent? dependent = null;
        if (appt.DependentId is not null)
        {
            dependent = await _dependents.FindByIdAsync(appt.DependentId.Value, ct);
        }

        return new AppointmentDto(
            Id: appt.Id,
            UserId: appt.UserId,
            DoctorId: appt.DoctorId,
            DoctorName: doctor.Name,
            DepartmentName: doctor.Department?.Name ?? string.Empty,
            AppointmentDateUtc: appt.StartAt.ToUniversalTime(),
            Status: appt.Status.ToString(),
            DependentId: dependent?.Id,
            DependentFullName: dependent?.FullName);
    }

    public async Task<IReadOnlyList<AdminAppointmentDto>> GetAdminAllAsync(CancellationToken ct)
    {
        return await _appointments.ListAdminDtosAsync(ct);
    }

    public async Task CancelAsync(Guid userId, Guid appointmentId, CancelAppointmentRequest request, CancellationToken ct)
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

        await CancelCoreAsync(appt, request.Reason, ct);
    }

    public async Task ForceCancelAsync(Guid appointmentId, string reason, CancellationToken ct)
    {
        var appt = await _appointments.FindByIdAsync(appointmentId, ct);
        if (appt is null)
        {
            throw new NotFoundException("Appointment not found.");
        }

        await CancelCoreAsync(appt, reason, ct);
    }

    private async Task CancelCoreAsync(Appointment appt, string? reason, CancellationToken ct)
    {
        var nowUtc = _clock.UtcNow;

        appt.CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        appt.CancelledAtUtc = nowUtc;

        _stateMachine.Transition(appt, AppointmentStatus.Cancelled, nowUtc);

        await _appointments.SaveChangesAsync(ct);

        // Slot boşaldıysa bekleme listesindeki sıradaki kişiye otomatik rezerv dene.
        await _waitlist.TryReserveFreedSlotAsync(appt.DoctorId, appt.StartAt, ct);
    }

    public async Task<AppointmentDto> RescheduleAsync(Guid userId, Guid appointmentId, RescheduleAppointmentRequest request, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("Unauthenticated.");

        var appt = await _appointments.FindByIdAsync(appointmentId, ct);
        if (appt is null)
            throw new NotFoundException("Randevu bulunamadı.");

        if (appt.UserId != userId)
            throw new ForbiddenException("Başka bir kullanıcının randevusu ertelenemez.");

        var nowUtc = _clock.UtcNow;

        // Only Pending or Approved can be rescheduled
        if (appt.Status is not (AppointmentStatus.Pending or AppointmentStatus.Approved))
            throw new ConflictException("Sadece bekleyen veya onaylanan randevular ertelenebilir.");

        // Must be at least 2 hours before the old appointment
        if (appt.StartAt <= nowUtc)
            throw new ConflictException("Geçmiş randevular ertelenemez.");
        if ((appt.StartAt - nowUtc) <= TimeSpan.FromHours(2))
            throw new ConflictException("Randevu başlangıcına 2 saat kala erteleme yapılamaz.");

        var oldStartAt = appt.StartAt;
        var doctorId = appt.DoctorId;

        // Validate new time — same rules as Create
        var newStartUtc = request.NewAppointmentDate.ToUniversalTime();
        if (newStartUtc <= nowUtc)
            throw new ConflictException("Yeni randevu zamanı gelecekte olmalıdır.");
        if (newStartUtc > nowUtc.AddYears(1))
            throw new ConflictException("En fazla 1 yıl içinde randevu alınabilir.");

        var newEndUtc = newStartUtc.AddMinutes(30);
        var localDate = DateOnly.FromDateTime(request.NewAppointmentDate.DateTime);
        var localStart = TimeOnly.FromDateTime(request.NewAppointmentDate.DateTime);
        var localEnd = localStart.AddMinutes(30);

        if (localDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            throw new ConflictException("Hafta sonu randevu alınamaz.");
        if (localStart.Minute % 30 != 0)
            throw new ConflictException("Randevu başlangıç zamanı 30 dakikalık slotlara hizalı olmalıdır.");
        if (await _holidays.ExistsAsync(localDate, ct))
            throw new ConflictException("Tatil günlerinde randevu alınamaz.");

        var doctor = await _doctors.FindByIdAsync(doctorId, ct)
            ?? throw new NotFoundException("Doktor bulunamadı.");
        if (!doctor.IsActive)
            throw new ConflictException("Doktor aktif değil.");

        var availability = await _availability.FindByDoctorIdAsync(doctorId, ct);
        var workStart = availability?.WorkStart ?? new TimeOnly(9, 0);
        var workEnd = availability?.WorkEnd ?? new TimeOnly(17, 0);

        if (localStart < workStart || localEnd > workEnd)
            throw new ConflictException("Randevu zamanı doktorun çalışma saatleri dışında.");
        if (availability?.LunchStart is not null && availability.LunchEnd is not null)
        {
            if (localStart < availability.LunchEnd.Value && localEnd > availability.LunchStart.Value)
                throw new ConflictException("Randevu zamanı doktorun öğle arası ile çakışıyor.");
        }

        var newDayStartUtc = new DateTimeOffset(request.NewAppointmentDate.Date, request.NewAppointmentDate.Offset).ToUniversalTime();
        var newDayEndUtc = newDayStartUtc.AddDays(1);

        var timeOffs = await _timeOffs.ListForDoctorBetweenAsync(doctorId, newDayStartUtc, newDayEndUtc, ct);
        if (timeOffs.Any(t => newStartUtc < t.EndAtUtc && newEndUtc > t.StartAtUtc))
            throw new ConflictException("Randevu zamanı doktorun izin dönemi ile çakışıyor.");

        await _uow.BeginAsync(ct, IsolationLevel.Serializable);
        try
        {
            await _appointments.RescheduleWithLockAsync(
                appointmentId, userId, doctorId,
                newStartUtc, newEndUtc, newDayStartUtc, newDayEndUtc, nowUtc, ct);
            await _uow.CommitAsync(ct);

            // Free the old slot for waitlist
            await _waitlist.TryReserveFreedSlotAsync(doctorId, oldStartAt, ct);

            // Send WhatsApp notification
            await TrySendRescheduleWhatsAppAsync(userId, doctor, request.NewAppointmentDate, ct);

            Dependent? dependent = null;
            if (appt.DependentId is not null)
                dependent = await _dependents.FindByIdAsync(appt.DependentId.Value, ct);

            return new AppointmentDto(
                Id: appt.Id,
                UserId: appt.UserId,
                DoctorId: appt.DoctorId,
                DoctorName: doctor.Name,
                DepartmentName: doctor.Department?.Name ?? string.Empty,
                AppointmentDateUtc: newStartUtc,
                Status: appt.Status.ToString(),
                DependentId: dependent?.Id,
                DependentFullName: dependent?.FullName);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            await _uow.RollbackAsync(ct);
            throw new NotFoundException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await _uow.RollbackAsync(ct);
            throw new ConflictException(ex.Message);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    private async Task TrySendRescheduleWhatsAppAsync(Guid userId, Doctor doctor, DateTimeOffset newDate, CancellationToken ct)
    {
        try
        {
            var patient = await _patients.FindByUserIdAsync(userId, ct);
            if (patient is null || string.IsNullOrWhiteSpace(patient.Phone)) return;

            var culture = CultureInfo.GetCultureInfo("tr-TR");
            var localDate = newDate.ToString("dd MMMM yyyy HH:mm", culture);
            var hospitalName = doctor.Department?.Hospital?.Name ?? string.Empty;
            var departmentName = doctor.Department?.Name ?? string.Empty;

            var message = string.Format(culture,
                "Randevunuz ertelendi. Hastane: {0}, Departman: {1}, Doktor: {2}, Yeni Tarih: {3}.",
                hospitalName, departmentName, doctor.Name, localDate);

            await _whatsappSender.SendMessageAsync(patient.Phone, message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reschedule WhatsApp. UserId={UserId}", userId);
        }
    }
}
