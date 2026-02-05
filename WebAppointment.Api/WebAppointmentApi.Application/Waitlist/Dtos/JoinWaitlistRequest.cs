namespace WebAppointmentApi.Application.Waitlist.Dtos;

public sealed record JoinWaitlistRequest(
    int DoctorId,
    DateTimeOffset StartAtUtc
);
