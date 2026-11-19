using System.Globalization;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Medicines;
using MediReminderBackend.Interfaces;
using MediReminderBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Services;

public class MedicineService : IMedicineService
{
    private readonly ApplicationDbContext _context;

    public MedicineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<MedicineListResponseDto>> GetMedicinesAsync(int userId, string? status, string? search)
    {
        var query = _context.Medicines
            .Include(m => m.Schedules)
            .Where(m => m.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower) || m.Dosage.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(m => m.Status.ToLower() == status.Trim().ToLower());
        }

        var allUserMedicines = await _context.Medicines
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var totalCount = allUserMedicines.Count;
        var activeCount = allUserMedicines.Count(m => m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
        var completedCount = allUserMedicines.Count(m => m.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

        var missedCount = await _context.MedicineHistories
            .CountAsync(h => h.UserId == userId && h.Status == "Missed");

        var medicineEntities = await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var dtos = medicineEntities.Select(MapToDto).ToList();

        var result = new MedicineListResponseDto
        {
            Summary = new MedicineSummaryDto
            {
                Total = totalCount,
                Active = activeCount,
                Completed = completedCount,
                Missed = missedCount
            },
            Medicines = dtos
        };

        return ApiResponse<MedicineListResponseDto>.Ok(result);
    }

    public async Task<ApiResponse<MedicineDto>> GetMedicineByIdAsync(int userId, int medicineId)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Schedules)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == medicineId);

        if (medicine == null)
        {
            return ApiResponse<MedicineDto>.Fail("Medicine not found.");
        }

        return ApiResponse<MedicineDto>.Ok(MapToDto(medicine));
    }

    public async Task<ApiResponse<List<MedicineDto>>> CreateMedicinesAsync(int userId, List<CreateMedicineDto> dtos)
    {
        if (dtos == null || dtos.Count == 0)
        {
            return ApiResponse<List<MedicineDto>>.Fail("No medicine details provided.");
        }

        var createdMedicines = new List<Medicine>();

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                continue;
            }

            DateOnly startDate = DateOnly.FromDateTime(DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(dto.StartDate) && DateOnly.TryParse(dto.StartDate, out var parsedStart))
            {
                startDate = parsedStart;
            }

            DateOnly? endDate = null;
            if (!string.IsNullOrWhiteSpace(dto.EndDate) && DateOnly.TryParse(dto.EndDate, out var parsedEnd))
            {
                endDate = parsedEnd;
            }

            var medicine = new Medicine
            {
                UserId = userId,
                Name = dto.Name.Trim(),
                Dosage = dto.Dosage.Trim(),
                Form = string.IsNullOrWhiteSpace(dto.Form) ? "Tablet" : dto.Form.Trim(),
                Frequency = string.IsNullOrWhiteSpace(dto.Frequency) ? "Once Daily" : dto.Frequency.Trim(),
                FoodInstruction = string.IsNullOrWhiteSpace(dto.FoodInstruction) ? "Take after food" : dto.FoodInstruction.Trim(),
                StartDate = startDate,
                EndDate = endDate,
                ReminderEnabled = dto.ReminderEnabled,
                Instructions = dto.Instructions,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
                CreatedAt = DateTime.UtcNow
            };

            var validTimes = dto.Times ?? new List<string>();
            if (validTimes.Count == 0)
            {
                validTimes.Add("08:00");
            }

            foreach (var timeStr in validTimes)
            {
                if (TimeSpan.TryParse(timeStr, out var parsedTime))
                {
                    medicine.Schedules.Add(new MedicineSchedule
                    {
                        ReminderTime = parsedTime
                    });
                }
            }

            createdMedicines.Add(medicine);
            _context.Medicines.Add(medicine);
        }

        await _context.SaveChangesAsync();

        var resultDtos = createdMedicines.Select(MapToDto).ToList();
        return ApiResponse<List<MedicineDto>>.Ok(resultDtos, $"{createdMedicines.Count} medicine(s) saved successfully.");
    }

    public async Task<ApiResponse<MedicineDto>> UpdateMedicineAsync(int userId, int medicineId, UpdateMedicineDto dto)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Schedules)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == medicineId);

        if (medicine == null)
        {
            return ApiResponse<MedicineDto>.Fail("Medicine not found.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            medicine.Name = dto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Dosage))
        {
            medicine.Dosage = dto.Dosage.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Form))
        {
            medicine.Form = dto.Form.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Frequency))
        {
            medicine.Frequency = dto.Frequency.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.FoodInstruction))
        {
            medicine.FoodInstruction = dto.FoodInstruction.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.StartDate) && DateOnly.TryParse(dto.StartDate, out var parsedStart))
        {
            medicine.StartDate = parsedStart;
        }

        if (!string.IsNullOrWhiteSpace(dto.EndDate) && DateOnly.TryParse(dto.EndDate, out var parsedEnd))
        {
            medicine.EndDate = parsedEnd;
        }

        medicine.ReminderEnabled = dto.ReminderEnabled;
        medicine.Instructions = dto.Instructions;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            medicine.Status = dto.Status.Trim();
        }

        // Update reminder schedules
        if (dto.Times != null && dto.Times.Count > 0)
        {
            _context.MedicineSchedules.RemoveRange(medicine.Schedules);
            medicine.Schedules.Clear();

            foreach (var timeStr in dto.Times)
            {
                if (TimeSpan.TryParse(timeStr, out var parsedTime))
                {
                    medicine.Schedules.Add(new MedicineSchedule
                    {
                        MedicineId = medicine.Id,
                        ReminderTime = parsedTime
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
        return ApiResponse<MedicineDto>.Ok(MapToDto(medicine), "Medicine updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteMedicineAsync(int userId, int medicineId)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == medicineId);

        if (medicine == null)
        {
            return ApiResponse<bool>.Fail("Medicine not found.");
        }

        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Medicine deleted successfully.");
    }

    private static MedicineDto MapToDto(Medicine medicine)
    {
        return new MedicineDto
        {
            Id = medicine.Id,
            Name = medicine.Name,
            Dosage = medicine.Dosage,
            Form = medicine.Form,
            Frequency = medicine.Frequency,
            FoodInstruction = medicine.FoodInstruction,
            StartDate = medicine.StartDate.ToString("yyyy-MM-dd"),
            EndDate = medicine.EndDate?.ToString("yyyy-MM-dd"),
            Times = medicine.Schedules
                .OrderBy(s => s.ReminderTime)
                .Select(s => s.ReminderTime.ToString(@"hh\:mm"))
                .ToList(),
            ReminderEnabled = medicine.ReminderEnabled,
            Instructions = medicine.Instructions,
            Status = medicine.Status
        };
    }
}
