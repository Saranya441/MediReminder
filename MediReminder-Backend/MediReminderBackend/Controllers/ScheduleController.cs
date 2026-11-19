using System.Security.Claims;
using MediReminderBackend.DTOs.Schedule;
using MediReminderBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
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

    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySchedule()
    {
        var userId = GetCurrentUserId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _scheduleService.GetScheduleForDateAsync(userId, today);
        return Ok(result);
    }

    [HttpGet("date")]
    public async Task<IActionResult> GetScheduleForDate([FromQuery] string date)
    {
        var userId = GetCurrentUserId();
        if (!DateOnly.TryParse(date, out var parsedDate))
        {
            return BadRequest(new { message = "Invalid date format. Expected yyyy-MM-dd." });
        }

        var result = await _scheduleService.GetScheduleForDateAsync(userId, parsedDate);
        return Ok(result);
    }

    [HttpPost("log-dose")]
    public async Task<IActionResult> LogDose([FromBody] LogDoseRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _scheduleService.LogDoseAsync(userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
