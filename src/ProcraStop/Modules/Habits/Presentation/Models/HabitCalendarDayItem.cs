using CommunityToolkit.Mvvm.ComponentModel;

namespace ProcraStop.Modules.Habits.Presentation.Models;

public partial class HabitCalendarDayItem : ObservableObject
{
    public DateTime Date { get; init; }
    public string DayName { get; init; } = string.Empty;
    public string DayNumber { get; init; } = string.Empty;
    public bool IsToday { get; init; }
    [ObservableProperty] private bool isSelected;
}
