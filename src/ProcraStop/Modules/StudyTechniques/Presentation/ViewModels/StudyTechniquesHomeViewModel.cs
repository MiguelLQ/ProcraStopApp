using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Application.Tasks.Facade;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Modules.StudyTechniques.Presentation.Models;
using ProcraStop.Modules.StudyTechniques.Presentation.Views;
using System.Collections.ObjectModel;

namespace ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;

public partial class StudyTechniquesHomeViewModel : ObservableObject
{
    private readonly StudyDashboardService _dashboardService;
    private readonly ITasksFacade _tasksFacade;

    public StudyTechniquesHomeViewModel(
        StudyDashboardService dashboardService,
        ITasksFacade tasksFacade)
    {
        _dashboardService = dashboardService;
        _tasksFacade = tasksFacade;
    }

    public ObservableCollection<StudyRescueTaskItem> RescueTasks { get; } = new();

    [ObservableProperty] private string sessionsThisWeekText = "0";
    [ObservableProperty] private string minutesThisWeekText = "0";
    [ObservableProperty] private string averageProgressText = "0%";
    [ObservableProperty] private string helpIndicatorText = "Sin datos";
    [ObservableProperty] private string mostUsedTechniqueText = "Sin datos";

    [RelayCommand]
    public async Task LoadAsync()
    {
        var dashboard = await _dashboardService.GetAsync(CancellationToken.None);
        SessionsThisWeekText = dashboard.SessionsThisWeek.ToString();
        MinutesThisWeekText = dashboard.MinutesThisWeek.ToString();
        AverageProgressText = $"{dashboard.AverageProgress}%";
        HelpIndicatorText = dashboard.HelpIndicatorText;
        MostUsedTechniqueText = dashboard.MostUsedTechnique;
        await LoadRescueTasksAsync();
    }

    [RelayCommand] public Task GoToRecommendAsync() => Shell.Current.GoToAsync(nameof(TechniqueRecommendationPage));
    [RelayCommand] public Task GoToLibraryAsync() => Shell.Current.GoToAsync(nameof(TechniqueLibraryPage));
    [RelayCommand] public Task GoToPomodoroAsync() => Shell.Current.GoToAsync(nameof(PomodoroPage));
    [RelayCommand] public Task GoToHistoryAsync() => Shell.Current.GoToAsync(nameof(StudySessionHistoryPage));

    [RelayCommand]
    public Task GoToTaskRecommendationAsync(StudyRescueTaskItem task)
    {
        var parameters = new Dictionary<string, object>
        {
            ["topic"] = task.Title
        };

        if (task.CourseId.HasValue)
            parameters["courseId"] = task.CourseId.Value.ToString();

        if (task.EstimatedMinutes.HasValue)
            parameters["minutes"] = task.EstimatedMinutes.Value.ToString();

        return Shell.Current.GoToAsync(nameof(TechniqueRecommendationPage), parameters);
    }

    private async Task LoadRescueTasksAsync()
    {
        var tasks = await _tasksFacade.GetTasksAsync(false, CancellationToken.None);
        var candidates = tasks
            .Where(t => t.Status != TaskItemStatus.Completed)
            .Where(t => t.IsOverdue || t.Priority == TaskPriority.High || t.EstimatedMinutes > 60)
            .OrderByDescending(t => t.IsOverdue)
            .ThenByDescending(t => t.Priority)
            .ThenByDescending(t => t.EstimatedMinutes ?? 0)
            .Take(3)
            .Select(t => new StudyRescueTaskItem(t));

        RescueTasks.Clear();
        foreach (var task in candidates)
            RescueTasks.Add(task);
    }
}
