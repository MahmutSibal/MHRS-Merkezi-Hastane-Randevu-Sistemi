using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using WebAppointmentApi.Application.Appointments.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _db;
    private readonly string _connectionString;
    private readonly ITenantContext _tenant;

    public AppointmentRepository(AppDbContext db, IConfiguration configuration, ITenantContext tenant)
    {
        _db = db;
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _tenant = tenant;
    }

    public async Task<Appointment> CreateWithLockAsync(
        Guid appointmentId,
        Guid userId,
        int doctorId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        CancellationToken ct)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
        }

        var tx = _db.Database.CurrentTransaction?.GetDbTransaction();
        if (tx is null)
        {
            throw new InvalidOperationException("UnitOfWork transaction is required for appointment creation.");
        }

                // Lock doctor row so concurrent bookings serialize per doctor and tenant.
                var doctorExists = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                @"SELECT 1
                                    FROM Doctors d WITH (UPDLOCK, HOLDLOCK)
                                    INNER JOIN Departments dep ON dep.Id = d.DepartmentId
                                    WHERE d.Id = @DoctorId AND d.IsActive = 1 AND dep.IsDeleted = 0 AND d.TenantId = @TenantId AND dep.TenantId = @TenantId;",
                                new { DoctorId = doctorId, TenantId = _tenant.TenantId },
                                transaction: tx,
                cancellationToken: ct));

        if (doctorExists is null)
        {
            throw new InvalidOperationException("Doctor not found.");
        }

        // Prevent overlapping appointments for doctor (Pending/Approved).
                var doctorConflict = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                @"SELECT TOP(1) 1
                  FROM Appointments WITH (UPDLOCK, HOLDLOCK)
                                    WHERE DoctorId = @DoctorId AND TenantId = @TenantId
                    AND Status IN (@Pending, @Approved)
                    AND @StartAtUtc < EndAt
                    AND @EndAtUtc > StartAt;",
                new
                {
                    DoctorId = doctorId,
                    StartAtUtc = startAtUtc,
                    EndAtUtc = endAtUtc,
                    Pending = (int)AppointmentStatus.Pending,
                    Approved = (int)AppointmentStatus.Approved,
                                        TenantId = _tenant.TenantId,
                },
                transaction: tx,
                cancellationToken: ct));

        if (doctorConflict is not null)
        {
            throw new InvalidOperationException("Doctor already booked for that time slot.");
        }

        // Prevent same user from overlapping (Pending/Approved).
                var userConflict = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(
                @"SELECT TOP(1) 1
                  FROM Appointments WITH (UPDLOCK, HOLDLOCK)
                                    WHERE UserId = @UserId AND TenantId = @TenantId
                    AND Status IN (@Pending, @Approved)
                    AND @StartAtUtc < EndAt
                    AND @EndAtUtc > StartAt;",
                new
                {
                    UserId = userId,
                    StartAtUtc = startAtUtc,
                    EndAtUtc = endAtUtc,
                    Pending = (int)AppointmentStatus.Pending,
                    Approved = (int)AppointmentStatus.Approved,
                                        TenantId = _tenant.TenantId,
                },
                transaction: tx,
                cancellationToken: ct));

        if (userConflict is not null)
        {
            throw new InvalidOperationException("User already has an appointment for that time slot.");
        }

        var createdAtUtc = DateTimeOffset.UtcNow;

        await connection.ExecuteAsync(
            new CommandDefinition(
                                @"INSERT INTO Appointments (Id, UserId, DoctorId, StartAt, EndAt, Status, CreatedAtUtc, UpdatedAtUtc, TenantId)
                                    VALUES (@Id, @UserId, @DoctorId, @StartAtUtc, @EndAtUtc, @Status, @CreatedAtUtc, NULL, @TenantId);",
                new
                {
                    Id = appointmentId,
                    UserId = userId,
                                        DoctorId = doctorId,
                    StartAtUtc = startAtUtc,
                    EndAtUtc = endAtUtc,
                    Status = (int)AppointmentStatus.Pending,
                    CreatedAtUtc = createdAtUtc,
                    TenantId = _tenant.TenantId,
                },
                transaction: tx,
                cancellationToken: ct));

                await connection.ExecuteAsync(
                        new CommandDefinition(
                                @"INSERT INTO AppointmentLogs (AppointmentId, Action, CreatedAtUtc, TenantId)
                                    VALUES (@AppointmentId, @Action, @CreatedAtUtc, @TenantId);",
                                new
                                {
                                        AppointmentId = appointmentId,
                                        Action = "Created",
                                        CreatedAtUtc = createdAtUtc,
                                        TenantId = _tenant.TenantId,
                                },
                                transaction: tx,
                                cancellationToken: ct));

                await connection.ExecuteAsync(
                        new CommandDefinition(
                                @"INSERT INTO Notifications (AppointmentId, UserId, Message, CreatedAtUtc, IsSent, TenantId)
                                    VALUES (@AppointmentId, @UserId, @Message, @CreatedAtUtc, 0, @TenantId);",
                                new
                                {
                                        AppointmentId = appointmentId,
                                        UserId = userId,
                                        Message = "Randevunuz oluşturuldu.",
                                        CreatedAtUtc = createdAtUtc,
                                        TenantId = _tenant.TenantId,
                                },
                                transaction: tx,
                                cancellationToken: ct));

        // Return via EF tracking for consistency, tolerate transient visibility.
        var appt = await _db.Appointments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == appointmentId, ct);
        if (appt is null)
        {
            throw new InvalidOperationException("Appointment not found after creation.");
        }
        return appt;
    }

    public async Task<IReadOnlyList<Appointment>> ListByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Appointments.AsNoTracking()
            .Include(x => x.Doctor)
            .ThenInclude(d => d!.Department)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.StartAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Appointment>> ListByDoctorIdAsync(int doctorId, CancellationToken ct)
    {
        return await _db.Appointments.AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.DoctorId == doctorId)
            .OrderByDescending(x => x.StartAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Appointment>> ListByDoctorIdBetweenAsync(
        int doctorId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken ct)
    {
        return await _db.Appointments.AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.StartAt >= fromUtc && x.StartAt < toUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Appointment>> ListAllAsync(CancellationToken ct)
    {
        return await _db.Appointments.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Doctor)
            .ThenInclude(d => d!.Department)
            .OrderByDescending(x => x.StartAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AppointmentDto>> ListMyDtosAsync(Guid userId, CancellationToken ct)
    {
        // Avoid AutoMapper ProjectTo with enum.ToString() (often not SQL-translatable).
        // Project a SQL-safe shape and convert status to string in memory.
        var rows = await _db.Appointments.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.StartAt)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.Name : string.Empty,
                DepartmentName = x.Doctor != null && x.Doctor.Department != null ? x.Doctor.Department.Name : string.Empty,
                AppointmentDateUtc = x.StartAt,
                x.Status,
            })
            .ToListAsync(ct);

        return rows.Select(x => new AppointmentDto(
            Id: x.Id,
            UserId: x.UserId,
            DoctorId: x.DoctorId,
            DoctorName: x.DoctorName,
            DepartmentName: x.DepartmentName,
            AppointmentDateUtc: x.AppointmentDateUtc,
            Status: x.Status.ToString()
        )).ToList();
    }

    public async Task<IReadOnlyList<AdminAppointmentDto>> ListAdminDtosAsync(CancellationToken ct)
    {
        // Avoid AutoMapper ProjectTo with enum.ToString() (often not SQL-translatable).
        // Also guard against Department query filter nulling required navigations.
        var rows = await _db.Appointments.AsNoTracking()
            .OrderByDescending(x => x.StartAt)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                UserEmail = x.User != null ? x.User.Email : string.Empty,
                x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.Name : string.Empty,
                DepartmentName = x.Doctor != null && x.Doctor.Department != null ? x.Doctor.Department.Name : string.Empty,
                AppointmentDateUtc = x.StartAt,
                x.Status,
            })
            .ToListAsync(ct);

        return rows.Select(x => new AdminAppointmentDto(
            Id: x.Id,
            UserId: x.UserId,
            UserEmail: x.UserEmail,
            DoctorId: x.DoctorId,
            DoctorName: x.DoctorName,
            DepartmentName: x.DepartmentName,
            AppointmentDateUtc: x.AppointmentDateUtc,
            Status: x.Status.ToString()
        )).ToList();
    }

    public Task<Appointment?> FindByIdAsync(Guid appointmentId, CancellationToken ct)
    {
        return _db.Appointments.SingleOrDefaultAsync(x => x.Id == appointmentId, ct);
    }

    public Task AddLogAsync(AppointmentLog log, CancellationToken ct)
    {
        _db.AppointmentLogs.Add(log);
        return Task.CompletedTask;
    }

    public Task AddNotificationAsync(Notification notification, CancellationToken ct)
    {
        _db.Notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
