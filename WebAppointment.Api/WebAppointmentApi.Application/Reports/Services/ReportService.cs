using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Reports.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.Application.Reports.Services;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reports;

    public ReportService(IReportRepository reports)
    {
        _reports = reports;
    }

    public Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, int? hospitalId, CancellationToken ct)
        => _reports.GetTopDoctorsLastDaysAsync(days, take, hospitalId, ct);

    public Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, int? hospitalId, CancellationToken ct)
        => _reports.GetAppointmentSummaryAsync(days, hospitalId, ct);
}
