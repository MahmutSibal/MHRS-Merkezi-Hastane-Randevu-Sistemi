namespace WebAppointmentApi.Application.Patients.Dtos;

// Hasta kaydı için e-posta tamamen kaldırıldı.
public sealed record CreatePatientNoEmailRequest(
    string Password,
    string TcKimlikNo,
    string FirstName,
    string LastName,
    string Phone,
    DateOnly BirthDate
);
