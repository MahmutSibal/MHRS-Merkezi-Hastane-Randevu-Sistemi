namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record DoctorPublicDetailDto(
    int Id,
    string Name,
    string Title,
    int DepartmentId,
    string DepartmentName,
    string ProfileStatus,
    string? GraduationUniversity,
    string? ExperienceSummary
);
