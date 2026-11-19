using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.Models;

public class UserProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(10)]
    public string? BloodGroup { get; set; }

    [MaxLength(150)]
    public string? EmergencyName { get; set; }

    [MaxLength(50)]
    public string? EmergencyMobile { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}
