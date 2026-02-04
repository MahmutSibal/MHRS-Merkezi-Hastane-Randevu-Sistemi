namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record UpdateDoctorRequest(
    string Name,
    string Title,
    int DepartmentId,
    bool IsActive,
    Guid? UserId
);
