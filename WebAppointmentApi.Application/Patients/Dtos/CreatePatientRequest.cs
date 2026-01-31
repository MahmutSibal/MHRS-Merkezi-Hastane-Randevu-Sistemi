namespace WebAppointmentApi.Application.Patients.Dtos;

public sealed record CreatePatientRequest(
    string Email,
    string Password,
    string TcKimlikNo,
    string FirstName,
    string LastName,
    string Phone
);
