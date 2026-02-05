namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record HospitalAdminPendingDoctorProfileDto(
    int DoctorId,
    string DoctorName,
    string DoctorTitle,
    int DepartmentId,
    string DepartmentName,
    string GraduationUniversity,
    string ExperienceSummary,
    DateTimeOffset SubmittedAtUtc
);
