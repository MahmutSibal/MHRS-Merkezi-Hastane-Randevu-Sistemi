using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Domain.Enums;
using WebAppointmentApi.Infrastructure.Data;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/dev")]
public sealed class DevController : ControllerBase
{
    private const string AdminEmail = "admin@hospital.local";
    private const string AdminPassword = "Admin123!";

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdmin(
        [FromServices] IWebHostEnvironment env,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        if (!env.IsDevelopment())
        {
            return NotFound();
        }

        var remote = HttpContext.Connection.RemoteIpAddress;
        if (remote is not null && !System.Net.IPAddress.IsLoopback(remote))
        {
            return Forbid();
        }

        var data = await db.Users
            .AsNoTracking()
            .Where(x => x.Email == AdminEmail)
            .Select(x => new
            {
                x.Id,
                x.Email,
                Role = x.Role.ToString(),
                HashFormat = x.PasswordHash.Contains('.') ? "pbkdf2" : "unknown",
                HashPrefix = x.PasswordHash.Length <= 25 ? x.PasswordHash : x.PasswordHash.Substring(0, 25)
            })
            .SingleOrDefaultAsync(ct);

        if (data is null)
        {
            return NotFound(new { Email = AdminEmail });
        }

        return Ok(data);
    }

    /// <summary>
    /// Dev-only: Ensures the admin user exists and resets its password to the default.
    /// Use this if the database was seeded externally with an incompatible hash format.
    /// </summary>
    [HttpPost("reset-admin")]
    public async Task<IActionResult> ResetAdmin(
        [FromServices] IWebHostEnvironment env,
        [FromServices] AppDbContext db,
        [FromServices] IPasswordHasher passwordHasher,
        CancellationToken ct)
    {
        if (!env.IsDevelopment())
        {
            return NotFound();
        }

        // Allow only local calls.
        var remote = HttpContext.Connection.RemoteIpAddress;
        if (remote is not null && !System.Net.IPAddress.IsLoopback(remote))
        {
            return Forbid();
        }

        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == AdminEmail, ct);
        if (user is null)
        {
            user = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = AdminEmail,
                Role = UserRole.Admin,
                PasswordHash = passwordHasher.Hash(AdminPassword),
            };

            db.Users.Add(user);
        }
        else
        {
            user.Role = UserRole.Admin;
            user.PasswordHash = passwordHasher.Hash(AdminPassword);
        }

        await db.SaveChangesAsync(ct);

        // Verify immediately using a fresh read (avoid any tracking surprises)
        var savedHash = await db.Users
            .AsNoTracking()
            .Where(x => x.Email == AdminEmail)
            .Select(x => x.PasswordHash)
            .SingleAsync(ct);

        var verifyOk = passwordHasher.Verify(AdminPassword, savedHash);

        return Ok(new
        {
            Email = AdminEmail,
            Password = AdminPassword,
            Role = UserRole.Admin.ToString(),
            HashFormat = savedHash.Contains('.') ? "pbkdf2" : "unknown",
            VerifyOk = verifyOk
        });
    }
}
