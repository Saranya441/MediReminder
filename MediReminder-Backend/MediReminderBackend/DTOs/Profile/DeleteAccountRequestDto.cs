using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Profile;

public class DeleteAccountRequestDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    public string Confirmation { get; set; } = string.Empty;
}
