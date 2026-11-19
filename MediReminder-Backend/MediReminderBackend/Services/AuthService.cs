using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Auth;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.Interfaces;
using MediReminderBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace MediReminderBackend.Services;

public class AuthService : IAuthService
{
    private static readonly IConfigurationManager<OpenIdConnectConfiguration> GoogleConfiguration =
        new ConfigurationManager<OpenIdConnectConfiguration>(
            "https://accounts.google.com/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());

    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IJwtService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var cleanIdentifier = request.Identifier.Trim();
        bool isEmail = cleanIdentifier.Contains('@');

        string? email = null;
        string? mobile = null;

        if (isEmail)
        {
            email = cleanIdentifier.ToLowerInvariant();
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists)
            {
                return ApiResponse<AuthResponseDto>.Fail("An account with this email address already exists.");
            }
        }
        else
        {
            mobile = cleanIdentifier;
            var mobileExists = await _context.Users.AnyAsync(u => u.MobileNumber == mobile);
            if (mobileExists)
            {
                return ApiResponse<AuthResponseDto>.Fail("An account with this mobile number already exists.");
            }
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            MobileNumber = mobile,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            Profile = new UserProfile
            {
                EmergencyName = null,
                EmergencyMobile = null
            },
            Settings = new UserSettings
            {
                MedicineReminders = true,
                MissedDoseAlerts = true,
                PrescriptionReminders = true,
                SoundEnabled = true,
                VibrationEnabled = true,
                AdvanceReminderMinutes = 10,
                Appearance = "system"
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id, user.FullName, user.Email, user.MobileNumber);

        var responseData = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber
            }
        };

        return ApiResponse<AuthResponseDto>.Ok(responseData, "Account created successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var cleanIdentifier = request.Identifier.Trim();
        bool isEmail = cleanIdentifier.Contains('@');

        User? user;
        if (isEmail)
        {
            var email = cleanIdentifier.ToLowerInvariant();
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        else
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.MobileNumber == cleanIdentifier);
        }

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid email/mobile number or password.");
        }

        var token = _jwtService.GenerateToken(user.Id, user.FullName, user.Email, user.MobileNumber);

        var responseData = new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber
            }
        };

        return ApiResponse<AuthResponseDto>.Ok(responseData, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(string credential)
    {
        var clientId = _configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return ApiResponse<AuthResponseDto>.Fail("Google sign-in is not configured. Set Google:ClientId in backend user secrets.");

        ClaimsPrincipal principal;
        try
        {
            var openId = await GoogleConfiguration.GetConfigurationAsync(CancellationToken.None);
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            principal = handler.ValidateToken(credential, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openId.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            }, out _);
        }
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
        {
            return ApiResponse<AuthResponseDto>.Fail("Google credential is invalid or expired. Please try again.");
        }

        var email = principal.FindFirst("email")?.Value?.Trim().ToLowerInvariant();
        var emailVerified = principal.FindFirst("email_verified")?.Value;
        if (string.IsNullOrWhiteSpace(email) || !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<AuthResponseDto>.Fail("Google must provide a verified email address to continue.");

        var user = await _context.Users.Include(u => u.Profile).Include(u => u.Settings)
            .SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            var fullName = principal.FindFirst("name")?.Value?.Trim();
            user = new User
            {
                FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                CreatedAt = DateTime.UtcNow,
                Profile = new UserProfile(),
                Settings = new UserSettings { Appearance = "system" }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = _jwtService.GenerateToken(user.Id, user.FullName, user.Email, user.MobileNumber);
        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Token = token,
            User = new UserDto { Id = user.Id, FullName = user.FullName, Email = user.Email, MobileNumber = user.MobileNumber }
        }, "Google sign-in successful.");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("User not found.");
        }

        return ApiResponse<UserDto>.Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber
        });
    }
}
