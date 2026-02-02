using WebAppointmentApi.Application.Appointments.Abstractions;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.Services;

public sealed class DoctorCalendarService : IDoctorCalendarService
{
    private static readonly TimeOnly WorkStart = new(9, 0);
    private static readonly TimeOnly WorkEnd = new(17, 0);
    private const int SlotMinutes = 30;

    private readonly IAppointmentRepository _appointments;
    private readonly IDoctorRepository _doctors;

    public DoctorCalendarService(IAppointmentRepository appointments, IDoctorRepository doctors)
    {
        _appointments = appointments;
        _doctors = doctors;
    }

    public async Task<IReadOnlyList<DoctorDailySlotDto>> GetMyDailySlotsAsync(Guid doctorUserId, DateOnly dateUtc, CancellationToken ct)
    {
        var doctor = await _doctors.FindByUserIdAsync(doctorUserId, ct);
        if (doctor is null)
        {
            throw new ForbiddenException("Doctor profile not found for current user.");
        }

        // IMPORTANT:
        // The UI expects working hours (09:00-17:00) in Turkey local time.
        // If we incorrectly mark 09:00 as UTC, the client may see it shifted (e.g. 12:00).
        // Build the day boundaries as LOCAL and convert to UTC for storage/query consistency.
        var localDayStart = DateTime.SpecifyKind(dateUtc.ToDateTime(WorkStart), DateTimeKind.Local);
        var localDayEnd = DateTime.SpecifyKind(dateUtc.ToDateTime(WorkEnd), DateTimeKind.Local);

        var dayStartUtc = new DateTimeOffset(localDayStart).ToUniversalTime();
        var dayEndUtc = new DateTimeOffset(localDayEnd).ToUniversalTime();

        var appts = await _appointments.ListByDoctorIdBetweenAsync(doctor.Id, dayStartUtc, dayEndUtc, ct);

        // Doküman: sadece Pending ve Approved zaman bloklar.
        var blocking = appts
            .Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Approved)
            .ToList();

        var slots = new List<DoctorDailySlotDto>();

        for (var startUtc = dayStartUtc; startUtc < dayEndUtc; startUtc = startUtc.AddMinutes(SlotMinutes))
        {
            var endUtc = startUtc.AddMinutes(SlotMinutes);
            var conflict = blocking.FirstOrDefault(a => startUtc < a.EndAt && endUtc > a.StartAt);

            slots.Add(new DoctorDailySlotDto(
                StartAtUtc: startUtc,
                EndAtUtc: endUtc,
                IsAvailable: conflict is null,
                AppointmentId: conflict?.Id,
                Status: conflict?.Status.ToString()));
        }

        return slots;
    }
}
