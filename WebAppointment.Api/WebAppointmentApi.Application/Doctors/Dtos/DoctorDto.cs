namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record DoctorDto(
    int Id,
    string Name,
    string Title,
    int DepartmentId,
    string DepartmentName,
    bool IsActive,
    Guid? UserId
);
