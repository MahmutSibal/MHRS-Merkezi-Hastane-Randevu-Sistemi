using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Hospitals.Abstractions;
using WebAppointmentApi.Application.Hospitals.Dtos;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Hospitals.Services;

public sealed class HospitalService : IHospitalService
{
    private readonly IHospitalRepository _hospitals;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public HospitalService(IHospitalRepository hospitals, IUserRepository users, IPasswordHasher passwordHasher)
    {
        _hospitals = hospitals;
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<HospitalDto>> ListAsync(CancellationToken ct)
    {
        var list = await _hospitals.ListAsync(ct);
        return list.Select(x => new HospitalDto(x.Id, x.Name, x.Address, x.Latitude, x.Longitude, x.Type)).ToList();
    }

    public async Task<IReadOnlyList<HospitalDto>> ListNearestAsync(double latitude, double longitude, int? take, CancellationToken ct)
    {
        var list = await _hospitals.ListAsync(ct);
        var withDistance = list
            .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
            .Select(x => new
            {
                Hospital = x,
                DistanceKm = Haversine(latitude, longitude, x.Latitude!.Value, x.Longitude!.Value)
            })
            .OrderBy(x => x.DistanceKm);

        var result = (take is not null ? withDistance.Take(take.Value) : withDistance)
            .Select(x => new HospitalDto(x.Hospital.Id, x.Hospital.Name, x.Hospital.Address, x.Hospital.Latitude, x.Hospital.Longitude, x.Hospital.Type))
            .ToList();

        return result;
    }

    public async Task<HospitalDto> CreateAsync(CreateHospitalRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ConflictException("Hospital name is required.");
        }

        var hospital = new Hospital
        {
            Name = request.Name.Trim(),
            Address = request.Address?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Type = request.Type,
            IsDeleted = false,
        };

        await _hospitals.AddAsync(hospital, ct);
        await _hospitals.SaveChangesAsync(ct);

        return new HospitalDto(hospital.Id, hospital.Name, hospital.Address, hospital.Latitude, hospital.Longitude, hospital.Type);
    }

    public async Task<Guid> AssignSubAdminAsync(int hospitalId, string email, string password, CancellationToken ct)
    {
        var hospital = await _hospitals.FindByIdAsync(hospitalId, ct);
        if (hospital is null)
        {
            throw new NotFoundException("Hospital not found.");
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new ConflictException("Email ve şifre zorunludur.");
        }

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var existing = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (existing is not null)
        {
            throw new ConflictException("Email zaten kayıtlı.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            PasswordHash = _passwordHasher.Hash(password),
            Role = UserRole.HospitalAdmin,
            HospitalId = hospitalId,
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task<IReadOnlyList<SubAdminDto>> ListSubAdminsAsync(int hospitalId, CancellationToken ct)
    {
        var hospital = await _hospitals.FindByIdAsync(hospitalId, ct);
        if (hospital is null)
        {
            throw new NotFoundException("Hospital not found.");
        }

        var users = await _users.ListHospitalAdminsByHospitalIdAsync(hospitalId, ct);
        return users.Select(x => new SubAdminDto(x.Id, x.Email)).ToList();
    }

    public async Task UpdateSubAdminCredentialsAsync(int hospitalId, Guid subAdminUserId, UpdateSubAdminCredentialsRequest request, CancellationToken ct)
    {
        var hospital = await _hospitals.FindByIdAsync(hospitalId, ct);
        if (hospital is null)
        {
            throw new NotFoundException("Hospital not found.");
        }

        var user = await _users.FindByIdAsync(subAdminUserId, ct);
        if (user is null || user.Role != UserRole.HospitalAdmin || user.HospitalId != hospitalId)
        {
            throw new NotFoundException("Alt admin bulunamadı.");
        }

        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
        var hasPassword = !string.IsNullOrWhiteSpace(request.Password);
        if (!hasEmail && !hasPassword)
        {
            throw new ConflictException("Email veya şifre zorunludur.");
        }

        if (hasEmail)
        {
            var newEmail = request.Email!.Trim();
            var normalizedEmail = newEmail.ToUpperInvariant();
            var existing = await _users.FindByEmailAsync(normalizedEmail, ct);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ConflictException("Email zaten kayıtlı.");
            }

            user.Email = newEmail;
        }

        if (hasPassword)
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password!);
        }

        await _users.SaveChangesAsync(ct);
    }

    public async Task DeleteSubAdminAsync(int hospitalId, Guid subAdminUserId, CancellationToken ct)
    {
        var hospital = await _hospitals.FindByIdAsync(hospitalId, ct);
        if (hospital is null)
        {
            throw new NotFoundException("Hospital not found.");
        }

        var user = await _users.FindByIdAsync(subAdminUserId, ct);
        if (user is null || user.Role != UserRole.HospitalAdmin || user.HospitalId != hospitalId)
        {
            throw new NotFoundException("Alt admin bulunamadı.");
        }

        await _users.DeleteAsync(user, ct);
        await _users.SaveChangesAsync(ct);
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0; // Earth radius in km
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180.0;
}
