namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record CreateDoctorRequest(
    string Name,
    int DepartmentId,
    string? Email,
    string? Password,
    Guid? UserId
);
