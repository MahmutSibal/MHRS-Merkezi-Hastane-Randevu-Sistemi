using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
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
    private readonly IPhoneVerificationRepository _phoneVerifications;
    private readonly IPhoneVerificationSender _phoneSender;

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
        ITenantContext tenant,
        IPhoneVerificationRepository phoneVerifications,
        IPhoneVerificationSender phoneSender)
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
        _phoneVerifications = phoneVerifications;
        _phoneSender = phoneSender;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        // TC ile hasta girişi
        if (!string.IsNullOrWhiteSpace(request.TcKimlikNo))
        {
            var tc = request.TcKimlikNo.Trim();
            var patient = await _patients.FindByTcAsync(tc, ct);
            if (patient is null)
            {
                _logger.LogInformation("Login failed: patient not found. TC={Tc}", tc);
                throw new UnauthorizedException("Kimlik veya şifre hatalı.");
            }

            var user = await _users.FindByIdAsync(patient.UserId, ct);
            if (user is null || user.Role != UserRole.Patient)
            {
                _logger.LogInformation("Login failed: user not found or role mismatch. TC={Tc}", tc);
                throw new UnauthorizedException("Kimlik veya şifre hatalı.");
            }

            var hash = user.PasswordHash ?? string.Empty;
            var okTc = _passwordHasher.Verify(request.Password, hash);
            if (!okTc)
            {
                _logger.LogInformation(
                    "Login failed: invalid password (TC login). UserId={UserId} Role={Role}",
                    user.Id,
                    user.Role.ToString());
                throw new UnauthorizedException("Kimlik veya şifre hatalı.");
            }

            _logger.LogInformation(
                "Login success (TC). UserId={UserId} Role={Role}",
                user.Id,
                user.Role.ToString());

            return await IssueTokensAsync(user, ct);
        }

        // E-posta ile yönetim/doktor girişi
        var normalizedEmail = (request.Email ?? string.Empty).Trim().ToUpperInvariant();

        await _loginSecurity.EnsureNotLockedAsync(normalizedEmail, ct);

        var userByEmail = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (userByEmail is null)
        {
            _logger.LogInformation("Login failed: user not found. Email={Email}", normalizedEmail);
            await _loginSecurity.RegisterFailureAsync(normalizedEmail, _clientInfo.IpAddress, ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        var hashEmail = userByEmail.PasswordHash ?? string.Empty;
        var hashFormat = hashEmail.Contains('.') ? "pbkdf2" : "unknown";
        var hashPrefix = hashEmail.Length <= 25 ? hashEmail : hashEmail.Substring(0, 25);

        var okEmail = _passwordHasher.Verify(request.Password, hashEmail);
        if (!okEmail)
        {
            _logger.LogInformation(
                "Login failed: invalid password. Email={Email} UserId={UserId} Role={Role} HashFormat={HashFormat} HashPrefix={HashPrefix}",
                userByEmail.Email,
                userByEmail.Id,
                userByEmail.Role.ToString(),
                hashFormat,
                hashPrefix);

            await _loginSecurity.RegisterFailureAsync(normalizedEmail, _clientInfo.IpAddress, ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        _logger.LogInformation(
            "Login success. Email={Email} UserId={UserId} Role={Role}",
            userByEmail.Email,
            userByEmail.Id,
            userByEmail.Role.ToString());

        await _loginSecurity.RegisterSuccessAsync(normalizedEmail, ct);

        return await IssueTokensAsync(userByEmail, ct);
    }

    public async Task<LoginResponse> RegisterAsync(CreatePatientRequest request, CancellationToken ct)
    {
        await _uow.BeginAsync(ct);
        try
        {
            var response = await RegisterCoreAsync(request, ct);
            await _uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RequestPhoneVerificationCodeAsync(PhoneVerificationRequest request, CancellationToken ct)
    {
        var normalizedPhone = NormalizePhone(request.Phone);
        var now = _clock.UtcNow;

        var latest = await _phoneVerifications.FindLatestByPhoneAsync(normalizedPhone, ct);
        if (latest is not null && latest.UsedAtUtc is null && latest.ExpiresAtUtc > now)
        {
            var remaining = (int)Math.Ceiling((latest.ExpiresAtUtc - now).TotalSeconds);
            throw new TooManyRequestsException($"Kod zaten gonderildi. {remaining} saniye sonra tekrar deneyin.");
        }

        var code = GenerateCode();
        var salt = CreateSalt();
        var hash = HashCode(code, salt);

        var entry = new PhoneVerificationCode
        {
            Id = Guid.NewGuid(),
            Phone = normalizedPhone,
            CodeHash = hash,
            CodeSalt = salt,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddSeconds(90),
            TenantId = _tenant.TenantId
        };

        await _phoneVerifications.AddAsync(entry, ct);
        await _phoneVerifications.SaveChangesAsync(ct);

        try
        {
            await _phoneSender.SendCodeAsync(normalizedPhone, code, ct);
        }
        catch (Exception ex)
        {
            entry.ExpiresAtUtc = now;
            await _phoneVerifications.SaveChangesAsync(ct);
            _logger.LogError(ex, "Failed to send phone verification code.");
            throw new ConflictException("Dogrulama kodu gonderilemedi. Lutfen tekrar deneyin.");
        }
    }

    public async Task<LoginResponse> RegisterWithPhoneVerificationAsync(RegisterWithPhoneVerificationRequest request, CancellationToken ct)
    {
        var normalizedPhone = NormalizePhone(request.Phone);
        var now = _clock.UtcNow;

        var latest = await _phoneVerifications.FindLatestByPhoneAsync(normalizedPhone, ct);
        if (latest is null || latest.UsedAtUtc is not null || latest.ExpiresAtUtc <= now)
        {
            throw new ValidationException("Dogrulama kodu suresi doldu. Lutfen yeniden isteyin.");
        }

        if (!IsCodeMatch(latest, request.Code))
        {
            latest.AttemptCount++;
            await _phoneVerifications.SaveChangesAsync(ct);
            throw new ValidationException("Dogrulama kodu hatali.");
        }

        await _uow.BeginAsync(ct);
        try
        {
            latest.UsedAtUtc = now;

            var mapped = new CreatePatientRequest(
                Email: null,
                Password: request.Password,
                TcKimlikNo: request.TcKimlikNo,
                FirstName: request.FirstName,
                LastName: request.LastName,
                Phone: request.Phone
            );

            var response = await RegisterCoreAsync(mapped, ct);

            await _uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<LoginResponse> RegisterCoreAsync(CreatePatientRequest request, CancellationToken ct)
    {
        // FluentValidation validates formatting. Here we enforce business constraints.
        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
        if (hasEmail)
        {
            var normalizedEmail = request.Email!.Trim().ToUpperInvariant();
            var emailExists = await _users.FindByEmailAsync(normalizedEmail, ct);
            if (emailExists is not null)
            {
                throw new ConflictException("Email zaten kayıtlı.");
            }
        }

        var tc = request.TcKimlikNo.Trim();
        var tcExists = await _patients.FindByTcAsync(tc, ct);
        if (tcExists is not null)
        {
            throw new ConflictException("Bu TC Kimlik No ile kayit zaten mevcut.");
        }

        var emailValue = hasEmail
            ? request.Email!.Trim()
            : string.Format(CultureInfo.InvariantCulture, "patient-{0}@mhrs.local", tc);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailValue,
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
        return response;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits.StartsWith("0", StringComparison.Ordinal))
        {
            digits = digits[1..];
        }

        return digits;
    }

    private static string GenerateCode()
        => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);

    private static string CreateSalt()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

    private static string HashCode(string code, string salt)
    {
        var input = string.Concat(salt, ":", code);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static bool IsCodeMatch(PhoneVerificationCode entry, string code)
        => string.Equals(entry.CodeHash, HashCode(code, entry.CodeSalt), StringComparison.Ordinal);

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
