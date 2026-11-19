using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Profile;

public class ChangePasswordRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}
