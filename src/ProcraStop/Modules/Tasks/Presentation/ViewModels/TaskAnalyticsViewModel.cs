

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProcraStop.Application.Tasks.Analytics;

namespace ProcraStop.Modules.Tasks.Presentation.ViewModels
{
    public partial class TaskAnalyticsViewModel : ObservableObject
    {
        private readonly TaskAnalyticsService _service;

        public TaskAnalyticsViewModel(TaskAnalyticsService service)
        {
            _service = service;
        }

        [ObservableProperty] private int total;
        [ObservableProperty] private int completed;
        [ObservableProperty] private int pending;
        [ObservableProperty] private int inProgress;
        [ObservableProperty] private int overdue;

        [ObservableProperty] private int highPriority;
        [ObservableProperty] private int mediumPriority;
        [ObservableProperty] private int lowPriority;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string completionRateText = "0%";
        [ObservableProperty] private string averageEffortText = "0 min";
        [ObservableProperty] private string performanceText = "Sin actividad";
        [ObservableProperty] private int completedThisWeek;
        [ObservableProperty] private int dueNextSevenDays;
        [ObservableProperty] private double[] weeklyValues = Array.Empty<double>();
        [ObservableProperty] private string[] weeklyLabels = Array.Empty<string>();
        [ObservableProperty] private double[] statusValues = Array.Empty<double>();
        [ObservableProperty] private string[] statusLabels = Array.Empty<string>();
        [ObservableProperty] private double[] priorityValues = Array.Empty<double>();
        [ObservableProperty] private string[] priorityLabels = Array.Empty<string>();
        [ObservableProperty] private double[] courseValues = Array.Empty<double>();
        [ObservableProperty] private string[] courseLabels = Array.Empty<string>();

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var data = await _service.GetAsync(CancellationToken.None);
                Total = data.Total;
                Completed = data.Completed;
                Pending = data.Pending;
                InProgress = data.InProgress;
                Overdue = data.Overdue;
                HighPriority = data.HighPriority;
                MediumPriority = data.MediumPriority;
                LowPriority = data.LowPriority;
                CompletedThisWeek = data.CompletedThisWeek;
                DueNextSevenDays = data.DueNextSevenDays;
                CompletionRateText = $"{data.CompletionRate:P0}";
                AverageEffortText = $"{data.AvgEffort:F0} min";
                PerformanceText = data.Total == 0
                    ? "Sin actividad"
                    : data.CompletionRate >= .75 ? "Ritmo excelente"
                    : data.CompletionRate >= .45 ? "Buen progreso"
                    : "Carga por atender";
                WeeklyValues = data.WeeklyCompleted;
                WeeklyLabels = data.WeeklyLabels;
                StatusValues = data.StatusValues;
                StatusLabels = data.StatusLabels;
                PriorityValues = data.PriorityValues;
                PriorityLabels = data.PriorityLabels;
                CourseValues = data.CourseValues;
                CourseLabels = data.CourseLabels;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
