using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IReportRepository
{
    Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, CancellationToken ct);
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, CancellationToken ct);
    Task<IReadOnlyList<NoShowRiskAppointmentDto>> GetNoShowRiskAppointmentsAsync(int days, int minScore, int? hospitalId, CancellationToken ct);
}
