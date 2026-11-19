namespace MediReminderBackend.DTOs.Medicines;

public class MedicineListResponseDto
{
    public MedicineSummaryDto Summary { get; set; } = new();
    public List<MedicineDto> Medicines { get; set; } = new();
}
