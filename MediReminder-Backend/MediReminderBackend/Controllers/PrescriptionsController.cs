using System.Security.Claims;
using MediReminderBackend.Data;
using MediReminderBackend.DTOs.Common;
using MediReminderBackend.DTOs.Prescriptions;
using MediReminderBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediReminderBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PrescriptionsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize + 64 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<PrescriptionDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<PrescriptionDto>.Fail("User identifier claim is missing or invalid."));
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<PrescriptionDto>.Fail("Choose a non-empty JPG, PNG, or PDF file."));
        if (file.Length > MaxFileSize)
            return BadRequest(ApiResponse<PrescriptionDto>.Fail("The file must be 10 MB or smaller."));

        var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        var (contentType, signatureValid) = await ValidateContentAsync(file, extension, cancellationToken);
        if (contentType == null || !signatureValid)
            return BadRequest(ApiResponse<PrescriptionDto>.Fail("Only valid JPG, PNG, or PDF files are accepted."));

        var safeStoredName = $"{Guid.NewGuid():N}{extension}";
        var userFolder = Path.Combine(_environment.ContentRootPath, "App_Data", "Prescriptions", userId.ToString());
        Directory.CreateDirectory(userFolder);
        var fullPath = Path.Combine(userFolder, safeStoredName);
        var relativePath = Path.Combine("App_Data", "Prescriptions", userId.ToString(), safeStoredName);

        try
        {
            await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await file.CopyToAsync(output, cancellationToken);

            var record = new Prescription
            {
                UserId = userId,
                FilePath = relativePath,
                OriginalFileName = Path.GetFileName(file.FileName),
                UploadedAt = DateTime.UtcNow,
                ProcessingStatus = "Pending"
            };
            _context.Prescriptions.Add(record);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = ToDto(record);
            return CreatedAtAction(nameof(GetById), new { id = record.Id },
                ApiResponse<PrescriptionDto>.Ok(dto, "Prescription uploaded. OCR processing is not enabled; review the document manually."));
        }
        catch
        {
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
            throw;
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PrescriptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<List<PrescriptionDto>>.Fail("User identifier claim is missing or invalid."));

        var prescriptions = await _context.Prescriptions.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<List<PrescriptionDto>>.Ok(prescriptions.Select(ToDto).ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<PrescriptionDto>.Fail("User identifier claim is missing or invalid."));

        var prescription = await _context.Prescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
        return prescription == null
            ? NotFound(ApiResponse<PrescriptionDto>.Fail("Prescription not found."))
            : Ok(ApiResponse<PrescriptionDto>.Ok(ToDto(prescription)));
    }

    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(ApiResponse<object>.Fail("User identifier claim is missing or invalid."));

        var prescription = await _context.Prescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
        if (prescription == null)
            return NotFound(ApiResponse<object>.Fail("Prescription not found."));

        var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "App_Data", "Prescriptions", userId.ToString()));
        var path = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, prescription.FilePath));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return NotFound(ApiResponse<object>.Fail("Prescription file not found."));
        if (!System.IO.File.Exists(path))
            return NotFound(ApiResponse<object>.Fail("Prescription file not found."));

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
        return PhysicalFile(path, contentType, Path.GetFileName(prescription.OriginalFileName), enableRangeProcessing: true);
    }

    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId);

    private PrescriptionDto ToDto(Prescription prescription) => new()
    {
        Id = prescription.Id,
        OriginalFileName = prescription.OriginalFileName,
        UploadedAt = prescription.UploadedAt,
        ProcessingStatus = prescription.ProcessingStatus,
        FileUrl = Url.ActionLink(nameof(Download), values: new { id = prescription.Id }) ?? $"/api/prescriptions/{prescription.Id}/file"
    };

    private static async Task<(string? ContentType, bool SignatureValid)> ValidateContentAsync(
        IFormFile file, string extension, CancellationToken cancellationToken)
    {
        var buffer = new byte[8];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
        var (expectedType, validSignature) = extension switch
        {
            ".jpg" or ".jpeg" => ("image/jpeg", read >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF),
            ".png" => ("image/png", read >= 8 && buffer.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })),
            ".pdf" => ("application/pdf", read >= 5 && buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46 && buffer[4] == 0x2D),
            _ => (null, false)
        };
        var typeMatches = string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase);
        return (typeMatches ? expectedType : null, validSignature && typeMatches);
    }
}
