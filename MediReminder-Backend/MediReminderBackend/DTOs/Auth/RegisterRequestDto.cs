using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email or mobile number is required.")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must contain at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}
