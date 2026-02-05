using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Auth.Abstractions;
using WebAppointmentApi.Application.Auth.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Common.Exceptions;
using WebAppointmentApi.Application.Patients.Dtos;
using WebAppointmentApi.Domain.Entities;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Application.Auth.Services;

/// <summary>
/// Authentication and token issuance service.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPatientRepository _patients;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokens;
    private readonly ILoginSecurityService _loginSecurity;
    private readonly IClientInfoProvider _clientInfo;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AuthService> _logger;
    private readonly ITenantContext _tenant;

    public AuthService(
        IUserRepository users,
        IPatientRepository patients,
        IPasswordHasher passwordHasher,
        ITokenService tokens,
        ILoginSecurityService loginSecurity,
        IClientInfoProvider clientInfo,
        IDateTimeProvider clock,
        IUnitOfWork uow,
        ILogger<AuthService> logger,
        ITenantContext tenant)
    {
        _users = users;
        _patients = patients;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
        _loginSecurity = loginSecurity;
        _clientInfo = clientInfo;
        _clock = clock;
        _uow = uow;
        _logger = logger;
        _tenant = tenant;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        await _loginSecurity.EnsureNotLockedAsync(normalizedEmail, ct);

        var user = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (user is null)
        {
            _logger.LogInformation("Login failed: user not found. Email={Email}", normalizedEmail);
            await _loginSecurity.RegisterFailureAsync(normalizedEmail, _clientInfo.IpAddress, ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var hash = user.PasswordHash ?? string.Empty;
        var hashFormat = hash.Contains('.') ? "pbkdf2" : "unknown";
        var hashPrefix = hash.Length <= 25 ? hash : hash.Substring(0, 25);

        var ok = _passwordHasher.Verify(request.Password, hash);
        if (!ok)
        {
            _logger.LogInformation(
                "Login failed: invalid password. Email={Email} UserId={UserId} Role={Role} HashFormat={HashFormat} HashPrefix={HashPrefix}",
                user.Email,
                user.Id,
                user.Role.ToString(),
                hashFormat,
                hashPrefix);

            await _loginSecurity.RegisterFailureAsync(normalizedEmail, _clientInfo.IpAddress, ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        _logger.LogInformation(
            "Login success. Email={Email} UserId={UserId} Role={Role}",
            user.Email,
            user.Id,
            user.Role.ToString());

        await _loginSecurity.RegisterSuccessAsync(normalizedEmail, ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<LoginResponse> RegisterAsync(CreatePatientRequest request, CancellationToken ct)
    {
        // FluentValidation validates formatting. Here we enforce business constraints.
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var emailExists = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (emailExists is not null)
        {
            throw new ConflictException("Email zaten kayıtlı.");
        }

        var tc = request.TcKimlikNo.Trim();
        var tcExists = await _patients.FindByTcAsync(tc, ct);
        if (tcExists is not null)
        {
            throw new ConflictException("Bu TC Kimlik No ile kay�t zaten mevcut.");
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
                TenantId = _tenant.TenantId,
            };

            await _users.AddAsync(user, ct);

            await _patients.AddAsync(new Patient
            {
                UserId = user.Id,
                TcKimlikNo = tc,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Phone = request.Phone.Trim(),
                TenantId = _tenant.TenantId,
            }, ct);

            await _users.SaveChangesAsync(ct);

            var response = await IssueTokensAsync(user, ct);

            await _uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var tokenHash = _tokens.HashRefreshToken(request.RefreshToken);
        var stored = await _users.FindRefreshTokenByHashAsync(tokenHash, ct);
        if (stored is null || stored.IsExpired || stored.IsRevoked)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var user = stored.User ?? await _users.FindByIdAsync(stored.UserId, ct);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        // Rotation: revoke old token, issue a new one.
        stored.RevokedAtUtc = _clock.UtcNow;

        var (accessToken, expiresAtUtc) = _tokens.CreateAccessToken(user);
        var newRefreshToken = _tokens.CreateRefreshToken();
        var newRefreshHash = _tokens.HashRefreshToken(newRefreshToken);

        var refreshDays = 30;
        var expiresRefreshAtUtc = _clock.UtcNow.AddDays(refreshDays);

        await _users.AddRefreshTokenAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshHash,
            CreatedAtUtc = _clock.UtcNow,
            ExpiresAtUtc = expiresRefreshAtUtc,
            TenantId = user.TenantId,
        }, ct);

        await _users.SaveChangesAsync(ct);

        return new LoginResponse(
            AccessToken: accessToken,
            AccessTokenExpiresAtUtc: expiresAtUtc,
            RefreshToken: newRefreshToken,
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role.ToString());
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var tokenHash = _tokens.HashRefreshToken(request.RefreshToken);
        var stored = await _users.FindRefreshTokenByHashAsync(tokenHash, ct);
        if (stored is null)
        {
            return;
        }

        if (!stored.IsRevoked)
        {
            stored.RevokedAtUtc = _clock.UtcNow;
            await _users.SaveChangesAsync(ct);
        }
    }

    public async Task<LoginResponse> UpdateMyCredentialsAsync(Guid userId, UpdateMyCredentialsRequest request, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId, ct);
        if (user is null)
        {
            throw new UnauthorizedException("Unauthorized");
        }

        var ok = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash ?? string.Empty);
        if (!ok)
        {
            throw new UnauthorizedException("Mevcut şifre yanlış.");
        }

        if (!string.IsNullOrWhiteSpace(request.NewEmail))
        {
            var newEmail = request.NewEmail.Trim();
            var normalizedEmail = newEmail.ToUpperInvariant();

            var existing = await _users.FindByEmailAsync(normalizedEmail, ct);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ConflictException("Email zaten kayıtlı.");
            }

            user.Email = newEmail;
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        }

        await _users.SaveChangesAsync(ct);

        // Issue new tokens so the caller immediately sees updated email claim.
        return await IssueTokensAsync(user, ct);
    }

    private async Task<LoginResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, expiresAtUtc) = _tokens.CreateAccessToken(user);
        var refreshToken = _tokens.CreateRefreshToken();
        var refreshTokenHash = _tokens.HashRefreshToken(refreshToken);

        var refreshDays = 30;
        var expiresRefreshAtUtc = _clock.UtcNow.AddDays(refreshDays);

        await _users.AddRefreshTokenAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = _clock.UtcNow,
            ExpiresAtUtc = expiresRefreshAtUtc,
            TenantId = user.TenantId,
        }, ct);

        await _users.SaveChangesAsync(ct);

        return new LoginResponse(
            AccessToken: accessToken,
            AccessTokenExpiresAtUtc: expiresAtUtc,
            RefreshToken: refreshToken,
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role.ToString());
    }
}
