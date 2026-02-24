namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record RegisterWithPhoneVerificationRequest(
    string Password,
    string TcKimlikNo,
    string FirstName,
    string LastName,
    string Phone,
    DateOnly BirthDate,
    string Code
);
