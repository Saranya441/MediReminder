namespace MediReminderBackend.DTOs.Medicines;

public class MedicineSummaryDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Completed { get; set; }
    public int Missed { get; set; }
}
