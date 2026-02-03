using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.Application.Reports.Abstractions;

public interface IReportService
{
    Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, int? hospitalId, CancellationToken ct);
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, int? hospitalId, CancellationToken ct);
}
