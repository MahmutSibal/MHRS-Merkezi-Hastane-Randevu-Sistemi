namespace WebAppointmentApi.Application.Patients.Dtos;

public sealed record UpdatePatientRequest(
    string Email,
    string FirstName,
    string LastName,
    string Phone
);
