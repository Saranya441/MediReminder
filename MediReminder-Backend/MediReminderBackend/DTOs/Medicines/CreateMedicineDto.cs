using System.ComponentModel.DataAnnotations;

namespace MediReminderBackend.DTOs.Medicines;

public class CreateMedicineDto
{
    private string _name = string.Empty;
    public string MedicineName
    {
        get => _name;
        set => _name = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    [Required(ErrorMessage = "Dosage is required.")]
    public string Dosage { get; set; } = string.Empty;

    public string Form { get; set; } = "Tablet";

    public string Frequency { get; set; } = "Once Daily";

    public string FoodInstruction { get; set; } = "Take after food";

    [Required(ErrorMessage = "Start date is required.")]
    public string StartDate { get; set; } = string.Empty;

    public string? EndDate { get; set; }

    private List<string> _times = new();
    public List<string> Timing
    {
        get => _times;
        set => _times = value;
    }

    public List<string> Times
    {
        get => _times;
        set => _times = value;
    }

    private bool _reminder = true;
    public bool Reminder
    {
        get => _reminder;
        set => _reminder = value;
    }

    public bool ReminderEnabled
    {
        get => _reminder;
        set => _reminder = value;
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => _notes = value;
    }

    public string? Instructions
    {
        get => _notes;
        set => _notes = value;
    }

    public string Status { get; set; } = "Active";
}
