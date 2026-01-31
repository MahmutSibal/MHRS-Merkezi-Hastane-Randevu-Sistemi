using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.State;

public interface IAppointmentStateMachine
{
    void Transition(Appointment appointment, AppointmentStatus targetStatus, DateTimeOffset nowUtc);
}
