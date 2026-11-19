using System.Globalization;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Schedule;
using MediReminderBackend.Interfaces;
using MediReminderBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Services;

public class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _context;

    public ScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ScheduleItemDto>>> GetScheduleForDateAsync(int userId, DateOnly date)
    {
        var medicines = await _context.Medicines
            .Include(m => m.Schedules)
            .Where(m => m.UserId == userId && m.Status == "Active" &&
                        m.StartDate <= date &&
                        (m.EndDate == null || m.EndDate >= date))
            .ToListAsync();

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        var existingHistories = await _context.MedicineHistories
            .Where(h => h.UserId == userId &&
                        h.ScheduledDateTime >= startOfDay &&
                        h.ScheduledDateTime <= endOfDay)
            .ToListAsync();

        var scheduleItems = new List<ScheduleItemDto>();
        var now = DateTime.UtcNow;

        foreach (var med in medicines)
        {
            foreach (var sched in med.Schedules)
            {
                var schedTimeOnly = TimeOnly.FromTimeSpan(sched.ReminderTime);
                var scheduledDateTime = date.ToDateTime(schedTimeOnly);

                var history = existingHistories.FirstOrDefault(h =>
                    h.MedicineId == med.Id &&
                    h.ScheduledDateTime.Hour == scheduledDateTime.Hour &&
                    h.ScheduledDateTime.Minute == scheduledDateTime.Minute);

                string status;
                DateTime? takenAt = null;

                if (history != null)
                {
                    status = history.Status;
                    takenAt = history.TakenAt;
                }
                else if (scheduledDateTime > now)
                {
                    status = "Upcoming";
                }
                else if (scheduledDateTime >= now.AddMinutes(-60))
                {
                    status = "Pending";
                }
                else
                {
                    status = "Missed";
                }

                scheduleItems.Add(new ScheduleItemDto
                {
                    Id = history?.Id ?? sched.Id,
                    MedicineId = med.Id,
                    MedicineName = med.Name,
                    Medicine = $"{med.Name} {med.Dosage}",
                    Dosage = med.Dosage,
                    Form = med.Form,
                    Quantity = $"1 {med.Form.ToLowerInvariant()}",
                    Date = date.ToString("yyyy-MM-dd"),
                    Time = scheduledDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                    ScheduledDateTime = scheduledDateTime,
                    FoodInstruction = med.FoodInstruction,
                    Instruction = med.FoodInstruction,
                    Status = status,
                    TakenAt = takenAt
                });
            }
        }

        scheduleItems = scheduleItems.OrderBy(s => s.ScheduledDateTime).ToList();
        return ApiResponse<List<ScheduleItemDto>>.Ok(scheduleItems);
    }

    public async Task<ApiResponse<CalendarResponseDto>> GetCalendarAsync(int userId, int month, int year, DateOnly selectedDate)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var firstDayOfMonth = new DateOnly(year, month, 1);
        var lastDayOfMonth = new DateOnly(year, month, daysInMonth);

        var medicines = await _context.Medicines
            .Include(m => m.Schedules)
            .Where(m => m.UserId == userId &&
                        m.StartDate <= lastDayOfMonth &&
                        (m.EndDate == null || m.EndDate >= firstDayOfMonth))
            .ToListAsync();

        var startDateTime = firstDayOfMonth.ToDateTime(TimeOnly.MinValue);
        var endDateTime = lastDayOfMonth.ToDateTime(TimeOnly.MaxValue);

        var histories = await _context.MedicineHistories
            .Where(h => h.UserId == userId &&
                        h.ScheduledDateTime >= startDateTime &&
                        h.ScheduledDateTime <= endDateTime)
            .ToListAsync();

        var markers = new Dictionary<string, List<string>>();
        var now = DateTime.UtcNow;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            var dateKey = date.ToString("yyyy-MM-dd");

            var activeMedsOnDay = medicines.Where(m =>
                m.StartDate <= date &&
                (m.EndDate == null || m.EndDate >= date) &&
                m.Schedules.Any()).ToList();

            if (!activeMedsOnDay.Any())
            {
                continue;
            }

            var daySchedules = new List<string>();
            foreach (var med in activeMedsOnDay)
            {
                foreach (var sched in med.Schedules)
                {
                    var schedDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(sched.ReminderTime));
                    var hist = histories.FirstOrDefault(h =>
                        h.MedicineId == med.Id &&
                        h.ScheduledDateTime.Date == date.ToDateTime(TimeOnly.MinValue).Date &&
                        h.ScheduledDateTime.Hour == schedDateTime.Hour &&
                        h.ScheduledDateTime.Minute == schedDateTime.Minute);

                    if (hist != null)
                    {
                        daySchedules.Add(hist.Status);
                    }
                    else if (schedDateTime > now)
                    {
                        daySchedules.Add("Upcoming");
                    }
                    else if (schedDateTime >= now.AddMinutes(-60))
                    {
                        daySchedules.Add("Pending");
                    }
                    else
                    {
                        daySchedules.Add("Missed");
                    }
                }
            }

            if (daySchedules.All(s => s == "Taken"))
            {
                markers[dateKey] = new List<string> { "has" };
            }
            else if (daySchedules.Any(s => s == "Missed"))
            {
                markers[dateKey] = new List<string> { "has", "missed" };
            }
            else if (daySchedules.Any(s => s == "Taken"))
            {
                markers[dateKey] = new List<string> { "has", "partial" };
            }
            else
            {
                markers[dateKey] = new List<string> { "has" };
            }
        }

        var selectedScheduleResult = await GetScheduleForDateAsync(userId, selectedDate);
        var selectedSchedule = selectedScheduleResult.Data ?? new List<ScheduleItemDto>();

        var takenCount = selectedSchedule.Count(s => s.Status == "Taken");
        var upcomingCount = selectedSchedule.Count(s => s.Status == "Upcoming" || s.Status == "Pending");
        var missedCount = selectedSchedule.Count(s => s.Status == "Missed");

        var response = new CalendarResponseDto
        {
            Markers = markers,
            SelectedSchedule = selectedSchedule,
            TakenCount = takenCount,
            UpcomingCount = upcomingCount,
            MissedCount = missedCount
        };

        return ApiResponse<CalendarResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<HistoryResponseDto>> GetHistoryAsync(int userId, string? status, string? dateFilter, string? selectedDateStr)
    {
        var query = _context.MedicineHistories
            .Include(h => h.Medicine)
            .Where(h => h.UserId == userId);

        var today = DateTime.UtcNow.Date;

        if (!string.IsNullOrWhiteSpace(selectedDateStr) && DateOnly.TryParse(selectedDateStr, out var selectedDate))
        {
            var start = selectedDate.ToDateTime(TimeOnly.MinValue);
            var end = selectedDate.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(h => h.ScheduledDateTime >= start && h.ScheduledDateTime <= end);
        }
        else if (!string.IsNullOrWhiteSpace(dateFilter))
        {
            if (dateFilter.Equals("Today", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(h => h.ScheduledDateTime.Date == today);
            }
            else if (dateFilter.Equals("Yesterday", StringComparison.OrdinalIgnoreCase))
            {
                var yesterday = today.AddDays(-1);
                query = query.Where(h => h.ScheduledDateTime.Date == yesterday);
            }
            else if (dateFilter.Equals("This Week", StringComparison.OrdinalIgnoreCase))
            {
                var diff = (7 + (today.DayOfWeek - DayOfWeek.Sunday)) % 7;
                var startOfWeek = today.AddDays(-diff);
                var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
                query = query.Where(h => h.ScheduledDateTime >= startOfWeek && h.ScheduledDateTime <= endOfWeek);
            }
            else if (dateFilter.Equals("This Month", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(h => h.ScheduledDateTime.Year == today.Year && h.ScheduledDateTime.Month == today.Month);
            }
            else if (dateFilter.Equals("Last Month", StringComparison.OrdinalIgnoreCase))
            {
                var lastMonth = today.AddMonths(-1);
                query = query.Where(h => h.ScheduledDateTime.Year == lastMonth.Year && h.ScheduledDateTime.Month == lastMonth.Month);
            }
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(h => h.Status.ToLower() == status.Trim().ToLower());
        }

        var histories = await query
            .OrderByDescending(h => h.ScheduledDateTime)
            .ToListAsync();

        var total = histories.Count;
        var taken = histories.Count(h => h.Status == "Taken");
        var missed = histories.Count(h => h.Status == "Missed");
        var skipped = histories.Count(h => h.Status == "Skipped");
        var adherence = total > 0 ? (int)Math.Round((double)taken / total * 100) : 100;

        var items = histories.Select(h => new HistoryItemDto
        {
            Id = h.Id,
            MedicineId = h.MedicineId,
            Medicine = $"{h.Medicine.Name} {h.Medicine.Dosage}",
            Dosage = h.Medicine.Dosage,
            Time = h.ScheduledDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture),
            Date = h.ScheduledDateTime.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture),
            RawDate = h.ScheduledDateTime.ToString("yyyy-MM-dd"),
            Instruction = h.Medicine.FoodInstruction,
            Status = h.Status,
            ScheduledDateTime = h.ScheduledDateTime,
            TakenAt = h.TakenAt,
            Notes = h.Notes
        }).ToList();

        var response = new HistoryResponseDto
        {
            TotalCount = total,
            TakenCount = taken,
            MissedCount = missed,
            SkippedCount = skipped,
            AdherencePercentage = adherence,
            History = items
        };

        return ApiResponse<HistoryResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<List<MissedDoseDto>>> GetMissedDosesAsync(int userId)
    {
        var missedHistories = await _context.MedicineHistories
            .Include(h => h.Medicine)
            .Where(h => h.UserId == userId && h.Status == "Missed")
            .OrderByDescending(h => h.ScheduledDateTime)
            .ToListAsync();

        var result = missedHistories.Select(h => new MissedDoseDto
        {
            Id = h.Id,
            MedicineId = h.MedicineId,
            Medicine = h.Medicine.Name,
            Dosage = h.Medicine.Dosage,
            Date = h.ScheduledDateTime.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture),
            RawDate = h.ScheduledDateTime.ToString("yyyy-MM-dd"),
            Time = h.ScheduledDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture),
            Instruction = h.Medicine.FoodInstruction,
            ScheduledDateTime = h.ScheduledDateTime
        }).ToList();

        return ApiResponse<List<MissedDoseDto>>.Ok(result);
    }

    public async Task<ApiResponse<ScheduleItemDto>> LogDoseAsync(int userId, LogDoseRequestDto request)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.UserId == userId && m.Id == request.MedicineId);

        if (medicine == null)
        {
            return ApiResponse<ScheduleItemDto>.Fail("Medicine not found or access denied.");
        }

        var existingHistory = await _context.MedicineHistories
            .FirstOrDefaultAsync(h => h.UserId == userId &&
                                      h.MedicineId == request.MedicineId &&
                                      h.ScheduledDateTime == request.ScheduledDateTime);

        DateTime? takenAt = request.Status.Equals("Taken", StringComparison.OrdinalIgnoreCase)
            ? DateTime.UtcNow
            : null;

        if (existingHistory != null)
        {
            existingHistory.Status = request.Status;
            existingHistory.TakenAt = takenAt;
            existingHistory.Notes = request.Notes;
        }
        else
        {
            existingHistory = new MedicineHistory
            {
                UserId = userId,
                MedicineId = request.MedicineId,
                ScheduledDateTime = request.ScheduledDateTime,
                Status = request.Status,
                TakenAt = takenAt,
                Notes = request.Notes
            };
            _context.MedicineHistories.Add(existingHistory);
        }

        await _context.SaveChangesAsync();

        var item = new ScheduleItemDto
        {
            Id = existingHistory.Id,
            MedicineId = medicine.Id,
            MedicineName = medicine.Name,
            Medicine = $"{medicine.Name} {medicine.Dosage}",
            Dosage = medicine.Dosage,
            Form = medicine.Form,
            Quantity = $"1 {medicine.Form.ToLowerInvariant()}",
            Date = request.ScheduledDateTime.ToString("yyyy-MM-dd"),
            Time = request.ScheduledDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture),
            ScheduledDateTime = request.ScheduledDateTime,
            FoodInstruction = medicine.FoodInstruction,
            Instruction = medicine.FoodInstruction,
            Status = request.Status,
            TakenAt = takenAt
        };

        return ApiResponse<ScheduleItemDto>.Ok(item, $"Dose marked as {request.Status}.");
    }
}
