using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class DoctorCalendarService : IDoctorCalendarService
{
    private static readonly TimeOnly DefaultWorkStart = new(9, 0);
    private static readonly TimeOnly DefaultWorkEnd = new(17, 0);
    private const int DefaultSlotMinutes = 30;

    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorRepository _doctors;
    private readonly IDoctorAvailabilityRepository _availability;
    private readonly IDoctorTimeOffRepository _timeOffs;
    private readonly IHolidayRepository _holidays;

    public DoctorCalendarService(
        IAppointmentRepository appointments,
        IDoctorRepository doctors,
        IDoctorAvailabilityRepository availability,
        IDoctorTimeOffRepository timeOffs,
        IHolidayRepository holidays)
    {
        _appointments = appointments;
        _doctors = doctors;
        _availability = availability;
        _timeOffs = timeOffs;
        _holidays = holidays;
    }

    public async Task<IReadOnlyList<DoctorDailySlotDto>> GetMyDailySlotsAsync(Guid doctorUserId, DateOnly dateUtc, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(doctorUserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        // dateUtc burada "takvim günü" anlamında kullanılıyor (UI local YYYY-MM-DD gönderiyor).
        // Resmi tatil ise hiç slot dönmeyelim.
        if (await _holidays.ExistsAsync(dateUtc, ct))
        {
            return Array.Empty<DoctorDailySlotDto>();
        }

        var availability = await _availability.FindByDoctorIdAsync(doctor.Id, ct);
        var workStart = availability?.WorkStart ?? DefaultWorkStart;
        var workEnd = availability?.WorkEnd ?? DefaultWorkEnd;
        var lunchStart = availability?.LunchStart;
        var lunchEnd = availability?.LunchEnd;
        var slotMinutes = availability?.SlotMinutes is > 0 and <= 180 ? availability!.SlotMinutes : DefaultSlotMinutes;

        // IMPORTANT:
        // The UI expects working hours (09:00-17:00) in Turkey local time.
        // If we incorrectly mark 09:00 as UTC, the client may see it shifted (e.g. 12:00).
        // Build the day boundaries as LOCAL and convert to UTC for storage/query consistency.
        var localDayStart = DateTime.SpecifyKind(dateUtc.ToDateTime(workStart), DateTimeKind.Local);
        var localDayEnd = DateTime.SpecifyKind(dateUtc.ToDateTime(workEnd), DateTimeKind.Local);

        var dayStartUtc = new DateTimeOffset(localDayStart).ToUniversalTime();
        var dayEndUtc = new DateTimeOffset(localDayEnd).ToUniversalTime();

        var timeOffs = await _timeOffs.ListForDoctorBetweenAsync(doctor.Id, dayStartUtc, dayEndUtc, ct);

        var appts = await _appointments.ListByDoctorIdBetweenAsync(doctor.Id, dayStartUtc, dayEndUtc, ct);

        // Dok�man: sadece Pending ve Approved zaman bloklar.
        var blocking = appts
            .Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Approved)
            .ToList();

        var slots = new List<DoctorDailySlotDto>();

        for (var startUtc = dayStartUtc; startUtc < dayEndUtc; startUtc = startUtc.AddMinutes(slotMinutes))
        {
            var endUtc = startUtc.AddMinutes(slotMinutes);
            var conflict = blocking.FirstOrDefault(a => startUtc < a.EndAt && endUtc > a.StartAt);

            var localStart = startUtc.ToLocalTime();
            var localEnd = endUtc.ToLocalTime();

            var isLunch = lunchStart is { } ls && lunchEnd is { } le
                && localStart.TimeOfDay < le.ToTimeSpan()
                && localEnd.TimeOfDay > ls.ToTimeSpan();

            var isTimeOff = timeOffs.Any(t => startUtc < t.EndAtUtc && endUtc > t.StartAtUtc);

            var isAvailable = conflict is null && !isLunch && !isTimeOff;

            slots.Add(new DoctorDailySlotDto(
                StartAtUtc: startUtc,
                EndAtUtc: endUtc,
                IsAvailable: isAvailable,
                AppointmentId: conflict?.Id,
                Status: conflict?.Status.ToString()));
        }

        return slots;
    }
}
