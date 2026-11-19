using System.Security.Claims;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Dashboard;
using MediReminderBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IScheduleService _scheduleService;

    public DashboardController(ApplicationDbContext context, IScheduleService scheduleService)
    {
        _context = context;
        _scheduleService = scheduleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var userId))
            return Unauthorized(ApiResponse<DashboardDto>.Fail("User identifier claim is missing or invalid."));

        // Existing scheduling data is stored as UTC DateTime values; use UTC consistently here.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var medicines = _context.Medicines.Where(m => m.UserId == userId);
        var total = await medicines.CountAsync();
        var active = await medicines.CountAsync(m => m.Status == "Active");
        var completed = await medicines.CountAsync(m => m.Status == "Completed");
        var allTimeMissed = await _context.MedicineHistories
            .CountAsync(h => h.UserId == userId && h.Status == "Missed");

        var scheduleResult = await _scheduleService.GetScheduleForDateAsync(userId, today);
        var schedule = scheduleResult.Data ?? new();
        var takenToday = schedule.Count(s => s.Status == "Taken");
        var pendingToday = schedule.Count(s => s.Status == "Pending");
        var upcomingToday = schedule.Count(s => s.Status == "Upcoming");
        var missedToday = schedule.Count(s => s.Status == "Missed");

        var weekStart = DateTime.UtcNow.Date.AddDays(-6);
        var weekEnd = DateTime.UtcNow;
        var weekHistories = await _context.MedicineHistories
            .Where(h => h.UserId == userId && h.ScheduledDateTime >= weekStart && h.ScheduledDateTime <= weekEnd)
            .Select(h => h.Status)
            .ToListAsync();
        var adherenceCount = weekHistories.Count(s => s == "Taken" || s == "Missed" || s == "Skipped");
        var adherence = adherenceCount == 0
            ? 100
            : (int)Math.Round((double)weekHistories.Count(s => s == "Taken") / adherenceCount * 100);

        var dto = new DashboardDto
        {
            Date = today,
            TotalMedicines = total,
            ActiveMedicines = active,
            CompletedMedicines = completed,
            MissedDoses = allTimeMissed,
            TodayDoseCount = schedule.Count,
            TakenToday = takenToday,
            PendingToday = pendingToday,
            UpcomingToday = upcomingToday,
            MissedToday = missedToday,
            AdherencePercentage = adherence,
            NextDose = schedule.FirstOrDefault(s => s.Status == "Pending" || s.Status == "Upcoming"),
            TodaySchedule = schedule
        };

        return Ok(ApiResponse<DashboardDto>.Ok(dto));
    }
}
