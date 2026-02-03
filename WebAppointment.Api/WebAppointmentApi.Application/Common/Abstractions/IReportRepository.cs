using WebAppointmentApi.Application.Reports.Dtos;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IReportRepository
{
    Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, CancellationToken ct);
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, CancellationToken ct);
}
