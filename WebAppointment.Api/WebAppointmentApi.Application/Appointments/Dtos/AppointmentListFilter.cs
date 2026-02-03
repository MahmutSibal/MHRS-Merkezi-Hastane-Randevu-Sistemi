using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Appointments.Dtos;

public sealed record AppointmentListFilter(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    AppointmentStatus? Status,
    int? HospitalId,
    int? Skip,
    int? Take);
