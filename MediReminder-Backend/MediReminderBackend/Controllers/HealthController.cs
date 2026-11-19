using MediReminderBackend.Data;
using MediReminderBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IJwtService jwtService, ApplicationDbContext dbContext, IWebHostEnvironment environment, ILogger<HealthController> logger)
    {
        _jwtService = jwtService;
        _dbContext = dbContext;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "Online",
            service = "MediReminder API",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    [HttpGet("database")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            return Ok(new
            {
                databaseConfigured = true,
                canConnect,
                provider = _dbContext.Database.ProviderName,
                message = canConnect
                    ? "Successfully connected to SQL Server database."
                    : "Database provider is registered, but SQL Server is currently unreachable. Check server name in appsettings.json."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL Server health check failed.");
            return Ok(new
            {
                databaseConfigured = true,
                canConnect = false,
                provider = _dbContext.Database.ProviderName,
                message = "Unable to connect to SQL Server. Ensure SQL Server service is running and server name in appsettings.json is correct."
            });
        }
    }

    [HttpGet("test-token")]
    [AllowAnonymous]
    public IActionResult GetTestToken([FromQuery] string name = "Saranya", [FromQuery] string email = "saranya@example.com")
    {
        if (!_environment.IsDevelopment())
            return NotFound();
        var token = _jwtService.GenerateToken(1, name, email, "+91 98765 43210");
        return Ok(new
        {
            token,
            expiresIn = "7 days",
            description = "Dev test token for Swagger and API testing"
        });
    }

    [HttpGet("protected")]
    [Authorize]
    public IActionResult GetProtectedStatus()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new
        {
            status = "Authorized",
            userId,
            userName,
            email,
            message = "JWT Bearer authentication is working correctly."
        });
    }
}
