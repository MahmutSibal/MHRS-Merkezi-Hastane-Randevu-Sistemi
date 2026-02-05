namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record CreateAppointmentRequest(
    int DoctorId,
    DateTimeOffset AppointmentDate,
    int? DependentId = null
);
