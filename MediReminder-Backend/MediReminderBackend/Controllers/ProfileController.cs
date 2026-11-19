using System.Security.Claims;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Profile;
using MediReminderBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProfileController> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return InvalidUser();
        var user = await _context.Users.AsNoTracking().Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user == null ? NotFound(ApiResponse<ProfileDto>.Fail("User not found.")) : Ok(ApiResponse<ProfileDto>.Ok(ToDto(user)));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return InvalidUser();
        var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return NotFound(ApiResponse<ProfileDto>.Fail("User not found."));
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        var mobile = string.IsNullOrWhiteSpace(request.MobileNumber) ? null : request.MobileNumber.Trim();
        if (email == null && mobile == null) return BadRequest(ApiResponse<ProfileDto>.Fail("Keep an email address or mobile number so you can sign in."));
        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow)) return BadRequest(ApiResponse<ProfileDto>.Fail("Date of birth cannot be in the future."));
        if (email != null && await _context.Users.AnyAsync(u => u.Id != userId && u.Email == email, cancellationToken)) return Conflict(ApiResponse<ProfileDto>.Fail("That email address is already in use."));
        if (mobile != null && await _context.Users.AnyAsync(u => u.Id != userId && u.MobileNumber == mobile, cancellationToken)) return Conflict(ApiResponse<ProfileDto>.Fail("That mobile number is already in use."));
        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.MobileNumber = mobile;
        user.Profile ??= new UserProfile { UserId = userId };
        user.Profile.DateOfBirth = request.DateOfBirth;
        user.Profile.Gender = EmptyToNull(request.Gender);
        user.Profile.BloodGroup = EmptyToNull(request.BloodGroup);
        user.Profile.EmergencyName = EmptyToNull(request.EmergencyName);
        user.Profile.EmergencyMobile = EmptyToNull(request.EmergencyMobile);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<ProfileDto>.Ok(ToDto(user), "Profile updated."));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return InvalidUser();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found."));
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash)) return BadRequest(ApiResponse<object>.Fail("Current password is incorrect."));
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash)) return BadRequest(ApiResponse<object>.Fail("Choose a new password."));
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Password changed."));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return InvalidUser();
        if (!string.Equals(request.Confirmation, "DELETE", StringComparison.Ordinal)) return BadRequest(ApiResponse<object>.Fail("Type DELETE exactly to confirm account deletion."));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found."));
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash)) return BadRequest(ApiResponse<object>.Fail("Current password is incorrect."));

        var relativeFiles = await _context.Prescriptions.Where(p => p.UserId == userId).Select(p => p.FilePath).ToListAsync(cancellationToken);
        var safeRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "App_Data", "Prescriptions", userId.ToString())) + Path.DirectorySeparatorChar;
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _context.MedicineHistories.RemoveRange(_context.MedicineHistories.Where(h => h.UserId == userId));
            await _context.SaveChangesAsync(cancellationToken);
            _context.Medicines.RemoveRange(_context.Medicines.Where(m => m.UserId == userId));
            _context.Prescriptions.RemoveRange(_context.Prescriptions.Where(p => p.UserId == userId));
            _context.UserProfiles.RemoveRange(_context.UserProfiles.Where(p => p.UserId == userId));
            _context.UserSettings.RemoveRange(_context.UserSettings.Where(s => s.UserId == userId));
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        foreach (var relativeFile in relativeFiles)
        {
            try
            {
                var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, relativeFile));
                if (fullPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not delete a prescription file for user {UserId}.", userId); }
        }
        return Ok(ApiResponse<object>.Ok(new { }, "Account and associated health data have been permanently deleted."));
    }

    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
    private IActionResult InvalidUser() => Unauthorized(ApiResponse<object>.Fail("User identifier claim is missing or invalid."));
    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static ProfileDto ToDto(User user) => new()
    {
        Id = user.Id, FullName = user.FullName, Email = user.Email, MobileNumber = user.MobileNumber,
        DateOfBirth = user.Profile?.DateOfBirth, Gender = user.Profile?.Gender, BloodGroup = user.Profile?.BloodGroup,
        EmergencyName = user.Profile?.EmergencyName, EmergencyMobile = user.Profile?.EmergencyMobile
    };
}
