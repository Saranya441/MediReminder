namespace MediReminderBackend.DTOs.Schedule;

public class HistoryResponseDto
{
    public int TotalCount { get; set; }
    public int TakenCount { get; set; }
    public int MissedCount { get; set; }
    public int SkippedCount { get; set; }
    public int AdherencePercentage { get; set; }
    public List<HistoryItemDto> History { get; set; } = new();
}
