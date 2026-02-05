namespace WebAppointmentApi.Application.Dependents.Dtos;

public sealed record DependentDto(
    int Id,
    string FullName,
    string TcKimlikNo
);
