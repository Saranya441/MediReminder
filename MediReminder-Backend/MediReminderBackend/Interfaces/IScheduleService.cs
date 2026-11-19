using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Schedule;

namespace MediReminderBackend.Interfaces;

public interface IScheduleService
{
    Task<ApiResponse<List<ScheduleItemDto>>> GetScheduleForDateAsync(int userId, DateOnly date);
    Task<ApiResponse<CalendarResponseDto>> GetCalendarAsync(int userId, int month, int year, DateOnly selectedDate);
    Task<ApiResponse<HistoryResponseDto>> GetHistoryAsync(int userId, string? status, string? dateFilter, string? selectedDate);
    Task<ApiResponse<List<MissedDoseDto>>> GetMissedDosesAsync(int userId);
    Task<ApiResponse<ScheduleItemDto>> LogDoseAsync(int userId, LogDoseRequestDto request);
}
