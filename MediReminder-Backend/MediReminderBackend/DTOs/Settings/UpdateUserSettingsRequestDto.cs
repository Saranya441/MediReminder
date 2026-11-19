using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Settings;

public class UpdateUserSettingsRequestDto
{
    public bool MedicineReminders { get; set; }
    public bool MissedDoseAlerts { get; set; }
    public bool PrescriptionReminders { get; set; }
    public bool SoundEnabled { get; set; }
    public bool VibrationEnabled { get; set; }

    [Range(0, 30)]
    public int AdvanceReminderMinutes { get; set; }

    [Required, RegularExpression("^(light|dark|system)$", ErrorMessage = "Appearance must be light, dark, or system.")]
    public string Appearance { get; set; } = "system";
}
