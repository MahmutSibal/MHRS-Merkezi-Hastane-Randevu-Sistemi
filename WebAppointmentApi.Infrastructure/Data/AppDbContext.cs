using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Domain.Entities;

namespace WebAppointmentApi.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentLog> AppointmentLogs => Set<AppointmentLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(320).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Patient>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TcKimlikNo).HasMaxLength(11).IsRequired();
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.HasQueryFilter(x => !x.IsDeleted);
            b.HasIndex(x => x.TcKimlikNo).IsUnique();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.IsDeleted).IsRequired();
            b.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Doctor>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.IsActive).IsRequired();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");

            b.HasOne(x => x.Department)
                .WithMany(x => x.Doctors)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.DepartmentId, x.IsActive });
        });

        modelBuilder.Entity<Appointment>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.StartAt).IsRequired();
            b.Property(x => x.EndAt).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.User)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Doctor)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.DoctorId, x.StartAt, x.EndAt });
            b.HasIndex(x => new { x.UserId, x.StartAt, x.EndAt });
            b.HasIndex(x => new { x.CreatedAtUtc, x.DoctorId });
        });

        modelBuilder.Entity<AppointmentLog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).HasMaxLength(100).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.HasIndex(x => new { x.AppointmentId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Message).HasMaxLength(500).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.IsSent).IsRequired();
            b.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.ExpiresAtUtc).IsRequired();

            b.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.UserId, x.TokenHash }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
