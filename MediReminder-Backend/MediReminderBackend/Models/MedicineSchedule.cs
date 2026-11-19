namespace MediReminderBackend.Models;

public class MedicineSchedule
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public TimeSpan ReminderTime { get; set; }

    // Navigation Property
    public Medicine Medicine { get; set; } = null!;
}
