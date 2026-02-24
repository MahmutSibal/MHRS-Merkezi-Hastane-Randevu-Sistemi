namespace WebAppointmentApi.Application.Patients.Dtos;

public sealed record PatientHealthProfileDto(
    string? BloodType,
    string? Allergies,
    string? ChronicDiseases,
    string? Medications,
    string? EmergencyContactName,
    string? EmergencyContactPhone);
