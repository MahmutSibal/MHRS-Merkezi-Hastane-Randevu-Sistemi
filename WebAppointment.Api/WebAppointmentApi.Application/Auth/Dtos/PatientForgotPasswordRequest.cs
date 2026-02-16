namespace WebAppointmentApi.Application.Auth.Dtos;

public sealed record PatientForgotPasswordRequest(
    string FirstName,
    string LastName,
    string TcKimlikNo,
    string Phone);