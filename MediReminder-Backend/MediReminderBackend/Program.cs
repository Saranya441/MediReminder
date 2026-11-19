using System.Text;
using System.Security.Cryptography;
using MediReminderBackend.Data;
using MediReminderBackend.Helpers;
using MediReminderBackend.Interfaces;
using MediReminderBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// DataProtection configuration for local development keys
var keysFolder = Path.Combine(AppContext.BaseDirectory, "temp-keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// Ensure configuration loads from project or base directory
builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true, reloadOnChange: true);

// 1. Configure Entity Framework Core with Microsoft SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=MediReminderDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        );
    });
});

// 2. Configure JWT Settings
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
var secretKey = !string.IsNullOrWhiteSpace(jwtSettings.SecretKey)
    ? jwtSettings.SecretKey
    : builder.Configuration["JWT_SECRET_KEY"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Configure JwtSettings:SecretKey or JWT_SECRET_KEY before starting outside Development.");
    secretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
jwtSettings.SecretKey = secretKey;
builder.Services.Configure<JwtSettings>(options =>
{
    jwtSection.Bind(options);
    options.SecretKey = secretKey;
});
var key = Encoding.UTF8.GetBytes(secretKey);

// 2. Register Dependency Injection Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

// 3. Configure CORS for React Vite frontend
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 4. Configure Authentication & JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Add Controllers
builder.Services.AddControllers();

// 6. Configure Swagger with JWT Bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MediReminder Web API",
        Version = "v1",
        Description = "Backend REST API for MediReminder medication reminder and management system."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token below. Example: eyJhbGciOi..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in Development and local runs
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MediReminder API v1");
        c.RoutePrefix = "swagger";
    });
}

// Enable CORS before Authentication & Routing
app.UseCors("AllowFrontendApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
