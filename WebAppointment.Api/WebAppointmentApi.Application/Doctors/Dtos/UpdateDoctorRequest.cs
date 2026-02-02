namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record UpdateDoctorRequest(
    string Name,
    int DepartmentId,
    bool IsActive,
    Guid? UserId
);
