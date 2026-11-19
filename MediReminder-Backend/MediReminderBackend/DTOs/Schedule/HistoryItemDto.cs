namespace MediReminderBackend.DTOs.Schedule;

public class HistoryItemDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string Medicine { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string RawDate { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string Status { get; set; } = "Taken"; // Taken, Missed, Skipped
    public DateTime ScheduledDateTime { get; set; }
    public DateTime? TakenAt { get; set; }
    public string? Notes { get; set; }
}
