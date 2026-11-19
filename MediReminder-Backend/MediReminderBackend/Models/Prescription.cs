using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.Models;

public class Prescription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string ProcessingStatus { get; set; } = "Pending"; // Pending, Scanned, Confirmed

    // Navigation Property
    public User User { get; set; } = null!;
}
