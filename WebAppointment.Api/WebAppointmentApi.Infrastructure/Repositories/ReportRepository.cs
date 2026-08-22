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

    public async Task<IReadOnlyList<NoShowRiskAppointmentDto>> GetNoShowRiskAppointmentsAsync(
        int days, int minScore, int? hospitalId, CancellationToken ct)
    {
        if (days <= 0) days = 7;
        if (days > 30) days = 30;
        if (minScore < 0) minScore = 0;

        var nowUtc = DateTimeOffset.UtcNow;
        var toUtc = nowUtc.AddDays(days);

        var query =
            from a in _db.Appointments.AsNoTracking()
            join p in _db.Patients.AsNoTracking() on a.UserId equals p.UserId
            where a.Status == AppointmentStatus.Approved
                  && a.StartAt >= nowUtc && a.StartAt <= toUtc
                  && p.NoShowScore >= minScore
            select new { a, p };

        if (hospitalId is not null)
        {
            query = query.Where(x => x.a.Doctor!.Department!.HospitalId == hospitalId.Value);
        }

        return await query
            .OrderByDescending(x => x.p.NoShowScore)
            .ThenBy(x => x.a.StartAt)
            .Select(x => new NoShowRiskAppointmentDto(
                x.a.Id,
                x.p.FirstName + " " + x.p.LastName,
                x.p.Phone,
                x.p.NoShowScore,
                x.a.Doctor!.Name,
                x.a.Doctor!.Department!.Hospital!.Name,
                x.a.StartAt,
                x.a.ReminderConfirmedAtUtc != null))
            .ToListAsync(ct);
    }
}
