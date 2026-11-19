using MediReminderBackend.DTOs.Auth;
using MediReminderBackend.DTOs.Common;

namespace MediReminderBackend.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(string credential);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId);
}
