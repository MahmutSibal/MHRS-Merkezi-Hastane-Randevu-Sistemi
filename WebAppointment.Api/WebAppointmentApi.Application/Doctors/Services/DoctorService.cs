using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Doctors.Services;

public sealed class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctors;
    private readonly IDepartmentRepository _departments;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public DoctorService(
        IDoctorRepository doctors,
        IDepartmentRepository departments,
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow)
    {
        _doctors = doctors;
        _departments = departments;
        _users = users;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task<IReadOnlyList<DoctorDto>> ListAsync(CancellationToken ct)
    {
        var list = await _doctors.ListAsync(ct);
        return list.Select(x => new DoctorDto(
            x.Id,
            x.Name,
            x.DepartmentId,
            x.Department?.Name ?? string.Empty,
            x.IsActive,
            x.UserId)).ToList();
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ConflictException("Doctor name is required.");
        }

        var department = await _departments.FindByIdAsync(request.DepartmentId, ct);
        if (department is null)
        {
            throw new NotFoundException("Department not found.");
        }

        // UserId direkt verildiyse var mý kontrol et.
        if (request.UserId is not null)
        {
            var exists = await _users.ExistsByIdAsync(request.UserId.Value, ct);
            if (!exists)
            {
                throw new NotFoundException("User not found.");
            }
        }

        // Email/Password verilmiþse doktor için kullanýcý hesabý üret.
        // (UserId ile birlikte verilmesini desteklemiyoruz.)
        if (!string.IsNullOrWhiteSpace(request.Email) || !string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.UserId is not null)
            {
                throw new ConflictException("UserId ile birlikte Email/Password gönderilemez.");
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ConflictException("Email ve Password birlikte zorunludur.");
            }

            var normalizedEmail = request.Email.Trim().ToUpperInvariant();
            var existing = await _users.FindByEmailAsync(normalizedEmail, ct);
            if (existing is not null)
            {
                throw new ConflictException("Email zaten kayýtlý.");
            }

            await _uow.BeginAsync(ct);
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email.Trim(),
                    PasswordHash = _passwordHasher.Hash(request.Password),
                    Role = UserRole.Doctor,
                };

                await _users.AddAsync(user, ct);

                var doctorWithUser = new Doctor
                {
                    Name = request.Name.Trim(),
                    DepartmentId = request.DepartmentId,
                    IsActive = true,
                    UserId = user.Id,
                };

                await _doctors.AddAsync(doctorWithUser, ct);

                await _users.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);

                return new DoctorDto(
                    doctorWithUser.Id,
                    doctorWithUser.Name,
                    doctorWithUser.DepartmentId,
                    department.Name,
                    doctorWithUser.IsActive,
                    doctorWithUser.UserId);
            }
            catch
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }

        var doctor = new Doctor
        {
            Name = request.Name.Trim(),
            DepartmentId = request.DepartmentId,
            IsActive = true,
            UserId = request.UserId,
        };

        await _doctors.AddAsync(doctor, ct);
        await _doctors.SaveChangesAsync(ct);

        return new DoctorDto(
            doctor.Id,
            doctor.Name,
            doctor.DepartmentId,
            department.Name,
            doctor.IsActive,
            doctor.UserId);
    }

    public async Task<DoctorDto> UpdateAsync(int doctorId, UpdateDoctorRequest request, CancellationToken ct)
    {
        var doctor = await _doctors.FindByIdAsync(doctorId, ct);
        if (doctor is null)
        {
            throw new NotFoundException("Doctor not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ConflictException("Doctor name is required.");
        }

        var department = await _departments.FindByIdAsync(request.DepartmentId, ct);
        if (department is null)
        {
            throw new NotFoundException("Department not found.");
        }

        if (request.UserId is not null)
        {
            var exists = await _users.ExistsByIdAsync(request.UserId.Value, ct);
            if (!exists)
            {
                throw new NotFoundException("User not found.");
            }
        }

        doctor.Name = request.Name.Trim();
        doctor.DepartmentId = request.DepartmentId;
        doctor.IsActive = request.IsActive;
        doctor.UserId = request.UserId;

        await _doctors.SaveChangesAsync(ct);

        return new DoctorDto(
            doctor.Id,
            doctor.Name,
            doctor.DepartmentId,
            department.Name,
            doctor.IsActive,
            doctor.UserId);
    }

    public async Task DeleteAsync(int doctorId, CancellationToken ct)
    {
        var doctor = await _doctors.FindByIdAsync(doctorId, ct);
        if (doctor is null)
        {
            return;
        }

        doctor.IsActive = false;
        await _doctors.SaveChangesAsync(ct);
    }
}
