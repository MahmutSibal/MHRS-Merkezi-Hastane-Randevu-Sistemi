using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Reports.Dtos;
using WebAppointmentApi.Infrastructure.Data;

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
}
