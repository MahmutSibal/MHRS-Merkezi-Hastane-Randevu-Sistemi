using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IReportRepository
{
    Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, int? hospitalId, CancellationToken ct);
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, int? hospitalId, CancellationToken ct);
}
