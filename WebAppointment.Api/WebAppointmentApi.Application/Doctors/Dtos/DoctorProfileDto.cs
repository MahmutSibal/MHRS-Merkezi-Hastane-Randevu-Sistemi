namespace WebAppointmentApi.Application.Doctors.Dtos;

public sealed record DoctorProfileDto(
    int DoctorId,
    string? GraduationUniversity,
    string? ExperienceSummary,
    string ProfileStatus,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? ApprovedAtUtc
);
