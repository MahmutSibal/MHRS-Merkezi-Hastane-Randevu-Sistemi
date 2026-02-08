using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Common;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    private readonly ITenantContext _tenant;
    private readonly IUserContext? _user;
    private readonly IHttpContextAccessor? _http;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext tenant,
        IUserContext? user = null,
        IHttpContextAccessor? http = null) : base(options)
    {
        _tenant = tenant;
        _user = user;
        _http = http;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Dependent> Dependents => Set<Dependent>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentLog> AppointmentLogs => Set<AppointmentLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<LoginLockout> LoginLockouts => Set<LoginLockout>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<WaitlistEntry> Waitlist => Set<WaitlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Domain).HasMaxLength(200);
            b.Property(x => x.IsActive).IsRequired();
        });

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();

            b.Property(x => x.TenantId).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);

            b.HasOne(x => x.Hospital)
                .WithMany()
                .HasForeignKey(x => x.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Hospital>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Address).HasMaxLength(500);
            b.Property(x => x.Latitude);
            b.Property(x => x.Longitude);
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
            b.HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

            b.HasIndex(x => new { x.Latitude, x.Longitude });
            b.HasIndex(x => new { x.TenantId, x.Name });
            b.HasIndex(x => new { x.TenantId, x.Type });
        });

        modelBuilder.Entity<Patient>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TcKimlikNo).HasMaxLength(11).IsRequired();
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.NoShowScore).HasDefaultValue(0);
            b.HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);
            b.HasIndex(x => x.TcKimlikNo).IsUnique();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Dependent>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            b.Property(x => x.BirthDate).HasColumnType("date").IsRequired();
            b.Property(x => x.Relation).IsRequired();
            b.Property(x => x.TcKimlikNo).HasMaxLength(11).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.Property(x => x.TenantId).IsRequired();
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);

            b.HasOne(x => x.GuardianUser)
                .WithMany()
                .HasForeignKey(x => x.GuardianUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.GuardianUserId, x.TcKimlikNo }).IsUnique();
        });

        modelBuilder.Entity<Department>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
            b.HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

            b.HasOne(x => x.Hospital)
                .WithMany(x => x.Departments)
                .HasForeignKey(x => x.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.HospitalId, x.Name });
        });

        modelBuilder.Entity<DoctorAvailability>(b =>
        {
            b.HasKey(x => x.DoctorId);
            b.Property(x => x.WorkStart).IsRequired();
            b.Property(x => x.WorkEnd).IsRequired();
            b.Property(x => x.SlotMinutes).HasDefaultValue(30);

            b.Property(x => x.TenantId).IsRequired();
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);

            b.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorTimeOff>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.StartAtUtc).IsRequired();
            b.Property(x => x.EndAtUtc).IsRequired();
            b.Property(x => x.Reason).HasMaxLength(500);

            b.Property(x => x.TenantId).IsRequired();
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);

            b.HasIndex(x => new { x.TenantId, x.DoctorId, x.StartAtUtc });
        });

        modelBuilder.Entity<Holiday>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Date).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();

            b.Property(x => x.TenantId).IsRequired();
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);

            b.HasIndex(x => new { x.TenantId, x.Date }).IsUnique();
        });

        modelBuilder.Entity<Doctor>(b =>
        {
                        b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.GraduationUniversity).HasMaxLength(200);
            b.Property(x => x.ExperienceSummary).HasMaxLength(2000);
            b.Property(x => x.ProfileStatus).IsRequired().HasConversion<int>().HasDefaultValue(WebAppointmentApi.Domain.Enums.DoctorProfileStatus.Draft);
            b.Property(x => x.ProfileSubmittedAtUtc);
            b.Property(x => x.ProfileApprovedAtUtc);
            b.Property(x => x.ProfileApprovedByUserId);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.UserId).IsUnique().HasFilter("\"UserId\" IS NOT NULL");

            b.HasOne(x => x.Department)
                .WithMany(x => x.Doctors)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.DepartmentId, x.IsActive });
        });

        modelBuilder.Entity<Appointment>(b =>
        {
                        b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
            b.HasKey(x => x.Id);
            b.Property(x => x.StartAt).IsRequired();
            b.Property(x => x.EndAt).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.TenantId).IsRequired();

            b.HasOne(x => x.User)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Doctor)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Dependent)
                .WithMany()
                .HasForeignKey(x => x.DependentId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.TenantId, x.DoctorId, x.StartAt, x.EndAt });
            b.HasIndex(x => new { x.TenantId, x.UserId, x.StartAt, x.EndAt });
            b.HasIndex(x => new { x.TenantId, x.CreatedAtUtc, x.DoctorId });
        });

        modelBuilder.Entity<AppointmentLog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(100).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
            b.HasIndex(x => new { x.AppointmentId, x.CreatedAtUtc });
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Message).HasMaxLength(500).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.IsSent).IsRequired();
            b.Property(x => x.TenantId).IsRequired();
            b.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.ExpiresAtUtc).IsRequired();
            b.Property(x => x.TenantId).IsRequired();

            b.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.UserId, x.TokenHash }).IsUnique();
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Role).HasMaxLength(100);
            b.Property(x => x.Action).HasMaxLength(50).IsRequired();
            b.Property(x => x.Entity).HasMaxLength(200).IsRequired();
            b.Property(x => x.EntityId).HasMaxLength(200).IsRequired();
            b.Property(x => x.TimestampUtc).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Entity, x.TimestampUtc });
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        modelBuilder.Entity<WaitlistEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAtUtc });
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        modelBuilder.Entity<LoginLockout>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
            b.Property(x => x.FailedCount).IsRequired();
            b.Property(x => x.FirstFailedAtUtc).IsRequired();
            b.Property(x => x.LastFailedAtUtc).IsRequired();
            b.Property(x => x.LockedUntilUtc);
            b.Property(x => x.LastIpAddress).HasMaxLength(50);

            b.HasIndex(x => new { x.TenantId, x.NormalizedEmail }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.LockedUntilUtc });
            b.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = _user?.UserId;
        var role = _user?.Role;
        var ip = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
        {
            if (entry.Entity is AuditLog) continue;

            var entityName = entry.Entity.GetType().Name;
            var entityId = TryGetIdString(entry.Entity);
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            if (entry.Entity is IMultiTenant mt && mt.TenantId == 0)
            {
                mt.TenantId = _tenant.TenantId;
            }

            string? before = null;
            string? after = null;

            try
            {
                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var original = entry.OriginalValues.ToObject();
                    before = JsonSerializer.Serialize(original);
                }
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    after = JsonSerializer.Serialize(entry.Entity);
                }
            }
            catch { /* swallow serialization issues */ }

            auditEntries.Add(new AuditLog
            {
                TenantId = _tenant.TenantId,
                UserId = userId.HasValue && userId.Value != Guid.Empty ? userId : null,
                Role = role ?? string.Empty,
                Action = action,
                Entity = entityName,
                EntityId = entityId,
                Before = before,
                After = after,
                TimestampUtc = now,
                IpAddress = ip,
            });
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private static string TryGetIdString(object entity)
    {
        var prop = entity.GetType().GetProperty("Id");
        var val = prop?.GetValue(entity);
        return val?.ToString() ?? string.Empty;
    }
}
