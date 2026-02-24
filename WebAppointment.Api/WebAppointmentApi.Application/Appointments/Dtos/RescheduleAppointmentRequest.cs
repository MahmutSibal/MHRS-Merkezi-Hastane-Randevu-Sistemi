namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record RescheduleAppointmentRequest(DateTimeOffset NewAppointmentDate);
