using System.Security.Claims;
using MediReminderBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public CalendarController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User identifier claim is missing or invalid.");
    }

    [HttpGet]
    public async Task<IActionResult> GetCalendar([FromQuery] int? month, [FromQuery] int? year, [FromQuery] string? date)
    {
        var userId = GetCurrentUserId();
        var today = DateTime.UtcNow;

        int selectedMonth = month ?? today.Month;
        int selectedYear = year ?? today.Year;

        DateOnly selectedDate;
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var parsed))
        {
            selectedDate = parsed;
        }
        else
        {
            selectedDate = DateOnly.FromDateTime(today);
        }

        var result = await _scheduleService.GetCalendarAsync(userId, selectedMonth, selectedYear, selectedDate);
        return Ok(result);
    }
}
