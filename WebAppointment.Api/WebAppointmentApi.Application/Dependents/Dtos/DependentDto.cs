namespace WebAppointmentApi.Application.Dependents.Dtos;

public sealed record DependentDto(
    int Id,
    string FullName,
    string TcKimlikNo,
    DateOnly BirthDate,
    WebAppointmentApi.Domain.Enums.DependentRelation Relation
);
