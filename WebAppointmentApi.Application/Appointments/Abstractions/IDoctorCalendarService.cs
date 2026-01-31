using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Abstractions;

public interface IDoctorCalendarService
{
    Task<IReadOnlyList<DoctorDailySlotDto>> GetMyDailySlotsAsync(Guid doctorUserId, DateOnly dateUtc, CancellationToken ct);
}
