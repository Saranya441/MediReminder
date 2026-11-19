using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Schedule;

public class LogDoseRequestDto
{
    [Required]
    public int MedicineId { get; set; }

    [Required]
    public DateTime ScheduledDateTime { get; set; }

    [Required]
    public string Status { get; set; } = "Taken"; // Taken, Missed, Skipped

    public string? Notes { get; set; }
}
