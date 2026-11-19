using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.Models;

public class MedicineHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MedicineId { get; set; }

    public DateTime ScheduledDateTime { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Taken"; // Taken, Missed, Skipped

    public DateTime? TakenAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}
