using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;
using WebAppointmentApi.Infrastructure.Data;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _db;

    public ReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TopDoctorDto>> GetTopDoctorsLastDaysAsync(int days, int take, CancellationToken ct)
    {
        if (days <= 0) days = 30;
        if (take <= 0) take = 10;
        if (take > 100) take = 100;

        var fromUtc = DateTimeOffset.UtcNow.AddDays(-days);

        return await _db.Appointments
            .AsNoTracking()
            .Where(a => a.CreatedAtUtc >= fromUtc)
            .GroupBy(a => a.DoctorId)
            .Select(g => new
            {
                DoctorId = g.Key,
                AppointmentCount = g.Count(),
            })
            .OrderByDescending(x => x.AppointmentCount)
            .Take(take)
            .Join(
                _db.Doctors.AsNoTracking(),
                x => x.DoctorId,
                d => d.Id,
                (x, d) => new TopDoctorDto(x.DoctorId, d.Name, x.AppointmentCount))
            .ToListAsync(ct);
    }

    public async Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(int days, CancellationToken ct)
    {
        if (days <= 0) days = 30;
        if (days > 365) days = 365;

        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-(days - 1));
        var startUtc = new DateTimeOffset(startDate, TimeSpan.Zero);

        var baseQuery = _db.Appointments
            .AsNoTracking()
            .Where(a => a.CreatedAtUtc >= startUtc);

        var statusCounts = await baseQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int CountFor(AppointmentStatus status)
            => statusCounts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        var statusSummary = new AppointmentStatusSummaryDto(
            Pending: CountFor(AppointmentStatus.Pending),
            Approved: CountFor(AppointmentStatus.Approved),
            Completed: CountFor(AppointmentStatus.Completed),
            Cancelled: CountFor(AppointmentStatus.Cancelled),
            Total: statusCounts.Sum(x => x.Count));

        var dailyCountsRaw = await baseQuery
            .GroupBy(a => a.CreatedAtUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dailyLookup = dailyCountsRaw.ToDictionary(x => x.Date, x => x.Count);

        var dailyCounts = Enumerable.Range(0, days)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                return new DailyAppointmentCountDto(
                    Date: date.ToString("yyyy-MM-dd"),
                    Count: dailyLookup.TryGetValue(date, out var count) ? count : 0);
            })
            .ToList();

        return new AppointmentSummaryDto(days, statusSummary, dailyCounts);
    }
}
