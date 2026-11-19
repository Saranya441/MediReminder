namespace MediReminderBackend.DTOs.Prescriptions;

public class PrescriptionDto
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string ProcessingStatus { get; set; } = "Pending";
    public string FileUrl { get; set; } = string.Empty;
}
