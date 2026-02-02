namespace WebAppointmentApi.Application.Reports.Dtos;

public sealed record TopDoctorDto(
    int DoctorId,
    string DoctorName,
    int AppointmentCount
);
