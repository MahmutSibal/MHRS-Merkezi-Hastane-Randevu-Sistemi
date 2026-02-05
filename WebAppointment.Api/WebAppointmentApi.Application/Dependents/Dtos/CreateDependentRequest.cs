namespace WebAppointmentApi.Application.Dependents.Dtos;

public sealed record CreateDependentRequest(
    string FullName,
    string TcKimlikNo
);
