using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class PublicDoctorCalendarService : IPublicDoctorCalendarService
{
    private static readonly TimeOnly DefaultWorkStart = new(9, 0);
    private static readonly TimeOnly DefaultWorkEnd = new(17, 0);
    private const int DefaultSlotMinutes = 30;

    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorRepository _doctors;
    private readonly IDoctorAvailabilityRepository _availability;
    private readonly IDoctorTimeOffRepository _timeOffs;
    private readonly IHolidayRepository _holidays;

    public PublicDoctorCalendarService(
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

    public async Task<IReadOnlyList<DoctorDailySlotPublicDto>> GetDoctorDailySlotsAsync(int doctorId, DateOnly localDate, CancellationToken ct)
    {
        var doctor = await _doctors.FindByIdAsync(doctorId, ct);
        if (doctor is null || !doctor.IsActive)
        {
            throw new NotFoundException("Doctor not found.");
        }

        if (localDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return Array.Empty<DoctorDailySlotPublicDto>();
        }

        if (await _holidays.ExistsAsync(localDate, ct))
        {
            return Array.Empty<DoctorDailySlotPublicDto>();
        }

        var availability = await _availability.FindByDoctorIdAsync(doctor.Id, ct);
        var workStart = availability?.WorkStart ?? DefaultWorkStart;
        var workEnd = availability?.WorkEnd ?? DefaultWorkEnd;
        var lunchStart = availability?.LunchStart;
        var lunchEnd = availability?.LunchEnd;
        var slotMinutes = availability?.SlotMinutes is > 0 and <= 180 ? availability!.SlotMinutes : DefaultSlotMinutes;

        var tz = GetTurkeyTimeZone();

        var localDayStart = localDate.ToDateTime(workStart);
        var localDayEnd = localDate.ToDateTime(workEnd);

        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDayStart, tz);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(localDayEnd, tz);

        var timeOffs = await _timeOffs.ListForDoctorBetweenAsync(doctor.Id, new DateTimeOffset(dayStartUtc), new DateTimeOffset(dayEndUtc), ct);
        var appts = await _appointments.ListByDoctorIdBetweenAsync(doctor.Id, new DateTimeOffset(dayStartUtc), new DateTimeOffset(dayEndUtc), ct);

        var blocking = appts
            .Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Approved)
            .ToList();

        var slots = new List<DoctorDailySlotPublicDto>();

        for (var slotLocalStart = localDayStart; slotLocalStart < localDayEnd; slotLocalStart = slotLocalStart.AddMinutes(slotMinutes))
        {
            var slotLocalEnd = slotLocalStart.AddMinutes(slotMinutes);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(slotLocalStart, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(slotLocalEnd, tz);

            var startAtUtc = new DateTimeOffset(startUtc);
            var endAtUtc = new DateTimeOffset(endUtc);

            var conflict = blocking.FirstOrDefault(a => startAtUtc < a.EndAt && endAtUtc > a.StartAt);

            var isLunch = lunchStart is { } ls && lunchEnd is { } le
                && TimeOnly.FromDateTime(slotLocalStart) < le
                && TimeOnly.FromDateTime(slotLocalEnd) > ls;

            var isTimeOff = timeOffs.Any(t => startAtUtc < t.EndAtUtc && endAtUtc > t.StartAtUtc);

            var isAvailable = conflict is null && !isLunch && !isTimeOff;

            string? reason = null;
            if (!isAvailable)
            {
                if (conflict is not null) reason = "Dolu";
                else if (isLunch) reason = "Öğle arası";
                else if (isTimeOff) reason = "İzinli";
                else reason = "Uygun değil";
            }

            slots.Add(new DoctorDailySlotPublicDto(
                StartTime: TimeOnly.FromDateTime(slotLocalStart).ToString("HH:mm"),
                EndTime: TimeOnly.FromDateTime(slotLocalEnd).ToString("HH:mm"),
                IsAvailable: isAvailable,
                UnavailableReason: reason));
        }

        return slots;
    }

    private static TimeZoneInfo GetTurkeyTimeZone()
    {
        try
        {
            // Windows: "Turkey Standard Time"
            return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch
        {
            return TimeZoneInfo.Local;
        }
    }
}
