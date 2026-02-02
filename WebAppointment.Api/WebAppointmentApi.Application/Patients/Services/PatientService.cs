using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Patients.Abstractions;
using WebAppointmentApi.Application.Patients.Dtos;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Patients.Services;

public sealed class PatientService : IPatientService
{
    private readonly IUserRepository _users;
    private readonly IPatientRepository _patients;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public PatientService(
        IUserRepository users,
        IPatientRepository patients,
        IPasswordHasher passwordHasher,
        IUnitOfWork uow)
    {
        _users = users;
        _patients = patients;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var existingUser = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (existingUser is not null)
        {
            throw new ConflictException("Email zaten kayıtlı.");
        }

        var tc = request.TcKimlikNo.Trim();
        var existingPatient = await _patients.FindByTcAsync(tc, ct);
        if (existingPatient is not null)
        {
            throw new ConflictException("Bu TC Kimlik No ile kayıt zaten mevcut.");
        }

        await _uow.BeginAsync(ct);
        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = UserRole.Patient,
            };

            await _users.AddAsync(user, ct);

            var patient = new Patient
            {
                UserId = user.Id,
                TcKimlikNo = tc,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Phone = request.Phone.Trim(),
            };

            await _patients.AddAsync(patient, ct);

            await _users.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return new PatientDto(
                patient.Id,
                user.Id,
                user.Email,
                patient.TcKimlikNo,
                patient.FirstName,
                patient.LastName,
                patient.Phone);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<PatientDto>> ListAsync(CancellationToken ct)
    {
        var list = await _patients.ListAsync(ct);
        return list.Select(p => new PatientDto(
                p.Id,
                p.UserId,
                p.User?.Email ?? string.Empty,
                p.TcKimlikNo,
                p.FirstName,
                p.LastName,
                p.Phone))
            .ToList();
    }

    public async Task<PatientDto> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken ct)
    {
        var patient = await _patients.FindByIdAsync(id, ct);
        if (patient is null)
        {
            throw new NotFoundException("Patient not found.");
        }

        // Email kullanıcı tablosunda tutuluyor
        if (patient.User is null)
        {
            var user = await _users.FindByIdAsync(patient.UserId, ct);
            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            patient.User = user;
        }

        var newEmail = request.Email.Trim();
        var normalizedEmail = newEmail.ToUpperInvariant();
        var existingUser = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (existingUser is not null && existingUser.Id != patient.UserId)
        {
            throw new ConflictException("Email zaten kayıtlı.");
        }

        patient.User!.Email = newEmail;
        patient.FirstName = request.FirstName.Trim();
        patient.LastName = request.LastName.Trim();
        patient.Phone = request.Phone.Trim();

        await _patients.SaveChangesAsync(ct);

        return new PatientDto(
            patient.Id,
            patient.UserId,
            patient.User.Email,
            patient.TcKimlikNo,
            patient.FirstName,
            patient.LastName,
            patient.Phone);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var patient = await _patients.FindByIdAsync(id, ct);
        if (patient is null)
        {
            return;
        }

        await _patients.SoftDeleteAsync(patient, ct);
        await _patients.SaveChangesAsync(ct);
    }
}
