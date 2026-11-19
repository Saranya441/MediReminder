using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Medicines;

public class UpdateMedicineDto
{
    [Required(ErrorMessage = "Medicine name is required.")]
    public string Name { get; set; } = string.Empty;

    public string Dosage { get; set; } = string.Empty;

    public string Form { get; set; } = "Tablet";

    public string Frequency { get; set; } = "Once Daily";

    public string FoodInstruction { get; set; } = "Take after food";

    public string StartDate { get; set; } = string.Empty;

    public string? EndDate { get; set; }

    public List<string> Times { get; set; } = new();

    public bool ReminderEnabled { get; set; } = true;

    public string? Instructions { get; set; }

    public string Status { get; set; } = "Active";
}
