namespace WebAppointmentApi.Application.Dependents.Dtos;

public sealed record CreateDependentRequest(
    string FullName,
    string TcKimlikNo,
    DateOnly BirthDate,
    WebAppointmentApi.Domain.Enums.DependentRelation Relation
);
