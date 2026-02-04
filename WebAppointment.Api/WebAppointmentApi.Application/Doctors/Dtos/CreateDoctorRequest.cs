namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record CreateDoctorRequest(
    string Name,
    string Title,
    int DepartmentId,
    string? Email,
    string? Password,
    Guid? UserId
);
