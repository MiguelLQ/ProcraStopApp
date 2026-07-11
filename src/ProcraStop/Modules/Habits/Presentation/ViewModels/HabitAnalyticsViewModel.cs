using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Habits.Analytics;

namespace ProcraStop.Modules.Habits.Presentation.ViewModels;

public partial class HabitAnalyticsViewModel : ObservableObject
{
    private readonly HabitAnalyticsService _service;

    public HabitAnalyticsViewModel(HabitAnalyticsService service)
    {
        _service = service;
    }

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private int activeHabits;
    [ObservableProperty] private int scheduledToday;
    [ObservableProperty] private int completedToday;
    [ObservableProperty] private int completedThisWeek;
    [ObservableProperty] private int expectedThisWeek;
    [ObservableProperty] private int atRiskHabits;
    [ObservableProperty] private int currentStreak;
    [ObservableProperty] private int bestStreak;
    [ObservableProperty] private string consistencyText = "0%";
    [ObservableProperty] private string failureText = "0%";
    [ObservableProperty] private string momentumText = "Sin datos";
    [ObservableProperty] private double[] weeklyValues = Array.Empty<double>();
    [ObservableProperty] private string[] weeklyLabels = Array.Empty<string>();
    [ObservableProperty] private double[] statusValues = Array.Empty<double>();
    [ObservableProperty] private string[] statusLabels = Array.Empty<string>();
    [ObservableProperty] private double[] typeValues = Array.Empty<double>();
    [ObservableProperty] private string[] typeLabels = Array.Empty<string>();
    [ObservableProperty] private double[] riskValues = Array.Empty<double>();
    [ObservableProperty] private string[] riskLabels = Array.Empty<string>();

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var data = await _service.GetAsync(CancellationToken.None);
            ActiveHabits = data.ActiveHabits;
            ScheduledToday = data.ScheduledToday;
            CompletedToday = data.CompletedToday;
            CompletedThisWeek = data.CompletedThisWeek;
            ExpectedThisWeek = data.ExpectedThisWeek;
            AtRiskHabits = data.AtRiskHabits;
            CurrentStreak = data.CurrentStreak;
            BestStreak = data.BestStreak;
            ConsistencyText = $"{data.ConsistencyRate:P0}";
            FailureText = $"{data.FailureRate:P0}";
            MomentumText = data.ActiveHabits == 0
                ? "Sin datos"
                : data.ConsistencyRate >= .8 ? "Constancia alta"
                : data.ConsistencyRate >= .5 ? "Buen ritmo"
                : "Recuperar impulso";
            WeeklyValues = data.WeeklyCompleted;
            WeeklyLabels = data.WeeklyLabels;
            StatusValues = data.StatusValues;
            StatusLabels = data.StatusLabels;
            TypeValues = data.TypeValues;
            TypeLabels = data.TypeLabels;
            RiskValues = data.RiskValues;
            RiskLabels = data.RiskLabels;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
