namespace MediReminderBackend.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string fullName, string? email, string? mobileNumber);
}
