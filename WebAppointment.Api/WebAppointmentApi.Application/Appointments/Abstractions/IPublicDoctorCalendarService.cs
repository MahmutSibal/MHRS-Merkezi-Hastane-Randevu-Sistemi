using WebAppointmentApi.Application.Appointments.Dtos;

namespace WebAppointmentApi.Application.Appointments.Abstractions;

public interface IPublicDoctorCalendarService
{
    Task<IReadOnlyList<DoctorDailySlotPublicDto>> GetDoctorDailySlotsAsync(int doctorId, DateOnly localDate, CancellationToken ct);
}
