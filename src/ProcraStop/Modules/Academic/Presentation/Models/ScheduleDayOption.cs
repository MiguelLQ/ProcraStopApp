namespace ProcraStop.Modules.Academic.Presentation.Models;

public sealed partial class ScheduleDayOption : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public DayOfWeek DayOfWeek { get; init; }
    public string ShortName { get; init; } = string.Empty;
    public string DayNumber { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool isSelected;
}
