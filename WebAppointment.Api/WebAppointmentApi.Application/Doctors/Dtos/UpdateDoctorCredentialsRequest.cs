namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record UpdateDoctorCredentialsRequest(
    string? Email,
    string? Password
);
