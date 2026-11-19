using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? MobileNumber { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public UserProfile? Profile { get; set; }
    public UserSettings? Settings { get; set; }
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<MedicineHistory> MedicineHistories { get; set; } = new List<MedicineHistory>();
}
