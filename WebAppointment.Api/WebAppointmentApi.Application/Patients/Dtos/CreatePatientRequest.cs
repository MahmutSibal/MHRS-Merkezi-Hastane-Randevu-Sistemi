namespace WebAppointmentApi.Application.Patients.Dtos;

// Hasta kaydı için e-posta artık zorunlu değil.
public sealed record CreatePatientRequest(
    string? Email,
    string Password,
    string TcKimlikNo,
    string FirstName,
    string LastName,
    string Phone
);
