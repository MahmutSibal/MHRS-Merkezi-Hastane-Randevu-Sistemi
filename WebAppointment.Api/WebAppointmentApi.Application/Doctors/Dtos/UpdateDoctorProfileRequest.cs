namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record UpdateDoctorProfileRequest(
    string GraduationUniversity,
    string ExperienceSummary
);
