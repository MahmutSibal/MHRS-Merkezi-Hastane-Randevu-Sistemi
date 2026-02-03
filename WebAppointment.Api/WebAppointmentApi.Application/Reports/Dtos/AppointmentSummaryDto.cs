namespace WebAppointmentApi.Application.Reports.Dtos;

public sealed record AppointmentStatusSummaryDto(
    int Pending,
    int Approved,
    int Completed,
    int Cancelled,
    int Total);

public sealed record DailyAppointmentCountDto(
    string Date,
    int Count);

public sealed record AppointmentSummaryDto(
    int Days,
    AppointmentStatusSummaryDto StatusSummary,
    IReadOnlyList<DailyAppointmentCountDto> DailyCounts);
