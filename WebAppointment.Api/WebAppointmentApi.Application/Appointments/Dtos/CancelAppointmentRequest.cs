namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record CancelAppointmentRequest(
    string? Reason
);
