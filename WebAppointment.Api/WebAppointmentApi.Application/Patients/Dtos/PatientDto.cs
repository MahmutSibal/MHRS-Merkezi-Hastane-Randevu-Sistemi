namespace WebAppointmentApi.Application.Patients.Dtos;

public sealed record PatientDto(
    int Id,
    Guid UserId,
    string Email,
    string TcKimlikNo,
    string FirstName,
    string LastName,
    string Phone
);
