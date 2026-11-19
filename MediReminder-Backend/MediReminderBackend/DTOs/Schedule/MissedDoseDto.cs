namespace MediReminderBackend.DTOs.Schedule;

public class MissedDoseDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string Medicine { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string RawDate { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public DateTime ScheduledDateTime { get; set; }
}
