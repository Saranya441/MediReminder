namespace MediReminderBackend.DTOs.Schedule;

public class ScheduleItemDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Medicine { get; set; } = string.Empty; // Alias for UI compatibility
    public string Dosage { get; set; } = string.Empty;
    public string Form { get; set; } = "Tablet";
    public string Quantity { get; set; } = "1 tablet";
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public DateTime ScheduledDateTime { get; set; }
    public string FoodInstruction { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty; // Alias for UI compatibility
    public string Status { get; set; } = "Upcoming"; // Taken, Pending, Upcoming, Missed, Skipped
    public DateTime? TakenAt { get; set; }
}
