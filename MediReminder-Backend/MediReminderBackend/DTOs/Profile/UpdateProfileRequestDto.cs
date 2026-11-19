using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Profile;

public class UpdateProfileRequestDto
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;
    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }
    [StringLength(50)]
    public string? MobileNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [StringLength(20)]
    public string? Gender { get; set; }
    [StringLength(10)]
    public string? BloodGroup { get; set; }
    [StringLength(150)]
    public string? EmergencyName { get; set; }
    [StringLength(50)]
    public string? EmergencyMobile { get; set; }
}
