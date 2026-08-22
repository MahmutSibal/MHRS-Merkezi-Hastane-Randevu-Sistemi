namespace WebAppointmentApi.Application.Reports.Dtos;

public sealed record NoShowRiskAppointmentDto(
    Guid AppointmentId,
    string PatientName,
    string PatientPhone,
    int NoShowScore,
    string DoctorName,
    string HospitalName,
    DateTimeOffset StartAtUtc,
    bool ReminderConfirmed
);
