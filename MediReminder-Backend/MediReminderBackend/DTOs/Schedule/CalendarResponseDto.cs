namespace MediReminderBackend.DTOs.Schedule;

public class CalendarResponseDto
{
    public Dictionary<string, List<string>> Markers { get; set; } = new();
    public List<ScheduleItemDto> SelectedSchedule { get; set; } = new();
    public int TakenCount { get; set; }
    public int UpcomingCount { get; set; }
    public int MissedCount { get; set; }
}
