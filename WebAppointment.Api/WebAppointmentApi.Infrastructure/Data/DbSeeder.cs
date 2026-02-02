using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Ensure deterministic admin credentials exist (idempotent).
        const string adminEmail = "admin@hospital.local";
        const string adminPassword = "Admin123!";

        var adminUser = await db.Users.SingleOrDefaultAsync(x => x.Email == adminEmail, ct);
        if (adminUser is null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                PasswordHash = passwordHasher.Hash(adminPassword),
                Role = UserRole.Admin,
            };

            db.Users.Add(adminUser);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            // If the password was changed (or seeded differently earlier), reset it to the default
            // so login works consistently in dev/local.
            if (!passwordHasher.Verify(adminPassword, adminUser.PasswordHash))
            {
                adminUser.PasswordHash = passwordHasher.Hash(adminPassword);
                await db.SaveChangesAsync(ct);
            }
        }

        // Seed demo data only for an empty database (excluding the ensured admin above).
        if (await db.Departments.AnyAsync(ct) || await db.Doctors.AnyAsync(ct) || await db.Patients.AnyAsync(ct))
        {
            return;
        }

        var cardiology = new Department { Name = "Cardiology", IsDeleted = false };
        var dermatology = new Department { Name = "Dermatology", IsDeleted = false };
        db.Departments.AddRange(cardiology, dermatology);

        var doctorUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "doctor1@hospital.local",
            PasswordHash = passwordHasher.Hash("Doctor123!"),
            Role = UserRole.Doctor,
        };

        var patientUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "patient1@hospital.local",
            PasswordHash = passwordHasher.Hash("Patient123!"),
            Role = UserRole.Patient,
        };

        db.Users.AddRange(doctorUser, patientUser);

        db.Doctors.Add(new Doctor
        {
            Name = "Dr. John Doe",
            Department = cardiology,
            UserId = doctorUser.Id,
            IsActive = true,
        });

        // Geçerli örnek TC üretmek için basit ve geçerli bir değer kullanıyoruz.
        db.Patients.Add(new Patient
        {
            UserId = patientUser.Id,
            TcKimlikNo = "10000000146",
            FirstName = "Ali",
            LastName = "Yilmaz",
            Phone = "+90 555 000 00 00",
        });

        await db.SaveChangesAsync(ct);
    }
}
