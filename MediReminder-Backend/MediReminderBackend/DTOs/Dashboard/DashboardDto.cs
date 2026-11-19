using MediReminderBackend.DTOs.Schedule;

namespace MediReminderBackend.DTOs.Dashboard;

public class DashboardDto
{
    public DateOnly Date { get; set; }
    public int TotalMedicines { get; set; }
    public int ActiveMedicines { get; set; }
    public int CompletedMedicines { get; set; }
    public int MissedDoses { get; set; }
    public int TodayDoseCount { get; set; }
    public int TakenToday { get; set; }
    public int PendingToday { get; set; }
    public int UpcomingToday { get; set; }
    public int MissedToday { get; set; }
    public int AdherencePercentage { get; set; }
    public ScheduleItemDto? NextDose { get; set; }
    public List<ScheduleItemDto> TodaySchedule { get; set; } = new();
}
