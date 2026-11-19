namespace MediReminderBackend.DTOs.Settings;

public class UserSettingsDto
{
    public bool MedicineReminders { get; set; }
    public bool MissedDoseAlerts { get; set; }
    public bool PrescriptionReminders { get; set; }
    public bool SoundEnabled { get; set; }
    public bool VibrationEnabled { get; set; }
    public int AdvanceReminderMinutes { get; set; }
    public string Appearance { get; set; } = "system";
}
