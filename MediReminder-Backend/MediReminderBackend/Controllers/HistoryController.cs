using System.Security.Claims;
using MediReminderBackend.DTOs.Schedule;
using MediReminderBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public HistoryController(IScheduleService scheduleService)
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
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? status,
        [FromQuery] string? dateFilter,
        [FromQuery] string? selectedDate)
    {
        var userId = GetCurrentUserId();
        var result = await _scheduleService.GetHistoryAsync(userId, status, dateFilter, selectedDate);
        return Ok(result);
    }

    [HttpGet("missed")]
    public async Task<IActionResult> GetMissedMedicines()
    {
        var userId = GetCurrentUserId();
        var result = await _scheduleService.GetMissedDosesAsync(userId);
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
