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
            throw new ConflictException("Email and Password are required.");
        }

        var normalizedEmail = email.Trim().ToUpperInvariant();
        var existing = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (existing is not null)
        {
            throw new ConflictException("Email already in use.");
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
