using System.Security.Claims;
using System.Text.Json;
using MediReminderBackend.DTOs.Medicines;
using MediReminderBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicinesController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
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
    public async Task<IActionResult> GetMedicines([FromQuery] string? status, [FromQuery] string? search)
    {
        var userId = GetCurrentUserId();
        var result = await _medicineService.GetMedicinesAsync(userId, status, search);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMedicineById(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _medicineService.GetMedicineByIdAsync(userId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicines([FromBody] JsonElement payload)
    {
        var userId = GetCurrentUserId();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dtos = new List<CreateMedicineDto>();

        if (payload.ValueKind == JsonValueKind.Array)
        {
            dtos = JsonSerializer.Deserialize<List<CreateMedicineDto>>(payload.GetRawText(), options) ?? new();
        }
        else if (payload.ValueKind == JsonValueKind.Object)
        {
            var single = JsonSerializer.Deserialize<CreateMedicineDto>(payload.GetRawText(), options);
            if (single != null)
            {
                dtos.Add(single);
            }
        }
        else
        {
            return BadRequest(new { message = "Invalid JSON payload. Expected object or array." });
        }

        var result = await _medicineService.CreateMedicinesAsync(userId, dtos);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMedicine(int id, [FromBody] UpdateMedicineDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        var result = await _medicineService.UpdateMedicineAsync(userId, id, dto);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _medicineService.DeleteMedicineAsync(userId, id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
