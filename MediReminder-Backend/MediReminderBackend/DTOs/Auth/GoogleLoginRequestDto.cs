using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Auth;

public class GoogleLoginRequestDto
{
    [Required]
    public string Credential { get; set; } = string.Empty;
}
