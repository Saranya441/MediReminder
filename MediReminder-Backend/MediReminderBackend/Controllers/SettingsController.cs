using System.Security.Claims;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return InvalidUser();
        var settings = await _context.UserSettings.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        return settings == null
            ? NotFound(ApiResponse<UserSettingsDto>.Fail("Settings not found."))
            : Ok(ApiResponse<UserSettingsDto>.Ok(ToDto(settings)));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserSettingsRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return InvalidUser();
        if (request.AdvanceReminderMinutes is not (0 or 5 or 10 or 15 or 30))
            return BadRequest(ApiResponse<UserSettingsDto>.Fail("Advance reminder time must be 0, 5, 10, 15, or 30 minutes."));

        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (settings == null) return NotFound(ApiResponse<UserSettingsDto>.Fail("Settings not found."));
        settings.MedicineReminders = request.MedicineReminders;
        settings.MissedDoseAlerts = request.MissedDoseAlerts;
        settings.PrescriptionReminders = request.PrescriptionReminders;
        settings.SoundEnabled = request.SoundEnabled;
        settings.VibrationEnabled = request.VibrationEnabled;
        settings.AdvanceReminderMinutes = request.AdvanceReminderMinutes;
        settings.Appearance = request.Appearance.ToLowerInvariant();
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<UserSettingsDto>.Ok(ToDto(settings), "Settings updated."));
    }

    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);
    private IActionResult InvalidUser() => Unauthorized(ApiResponse<object>.Fail("User identifier claim is missing or invalid."));

    private static UserSettingsDto ToDto(Models.UserSettings settings) => new()
    {
        MedicineReminders = settings.MedicineReminders,
        MissedDoseAlerts = settings.MissedDoseAlerts,
        PrescriptionReminders = settings.PrescriptionReminders,
        SoundEnabled = settings.SoundEnabled,
        VibrationEnabled = settings.VibrationEnabled,
        AdvanceReminderMinutes = settings.AdvanceReminderMinutes,
        Appearance = settings.Appearance
    };
}
