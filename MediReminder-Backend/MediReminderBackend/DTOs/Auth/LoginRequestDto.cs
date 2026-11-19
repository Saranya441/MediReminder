using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email or mobile number is required.")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
