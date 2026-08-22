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

    public Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, CancellationToken ct)
        => _reports.GetTopDoctorsLastDaysAsync(days, take, ct);

    public Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, CancellationToken ct)
        => _reports.GetAppointmentSummaryAsync(days, ct);

    public Task<IReadOnlyList<NoShowRiskAppointmentDto>> GetNoShowRiskAppointmentsAsync(int days, int minScore, int? hospitalId, CancellationToken ct)
        => _reports.GetNoShowRiskAppointmentsAsync(days, minScore, hospitalId, ct);
}
