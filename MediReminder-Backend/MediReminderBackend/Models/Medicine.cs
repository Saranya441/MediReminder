using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.Models;

public class Medicine
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Form { get; set; } = "Tablet";

    [MaxLength(50)]
    public string Frequency { get; set; } = "Once Daily";

    [MaxLength(100)]
    public string FoodInstruction { get; set; } = "Take after food";

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool ReminderEnabled { get; set; } = true;

    [MaxLength(500)]
    public string? Instructions { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public ICollection<MedicineSchedule> Schedules { get; set; } = new List<MedicineSchedule>();
    public ICollection<MedicineHistory> Histories { get; set; } = new List<MedicineHistory>();
}
