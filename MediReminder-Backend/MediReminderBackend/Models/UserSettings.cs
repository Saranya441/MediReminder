using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.Models;

public class UserSettings
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool MedicineReminders { get; set; } = true;

    public bool MissedDoseAlerts { get; set; } = true;

    public bool PrescriptionReminders { get; set; } = true;

    public bool SoundEnabled { get; set; } = true;

    public bool VibrationEnabled { get; set; } = true;

    public int AdvanceReminderMinutes { get; set; } = 10;

    [Required]
    [MaxLength(20)]
    public string Appearance { get; set; } = "system";

    // Navigation Property
    public User User { get; set; } = null!;
}
