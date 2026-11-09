

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

        // D1 EPA: postergacion al ultimo momento.
        [ObservableProperty] private string firstProgressAverageText = "Sin datos";
        [ObservableProperty] private string startDelayAverageText = "Sin datos";
        [ObservableProperty] private string onTimeStartRateText = "Sin datos";
        [ObservableProperty] private string lastMinuteStartRateText = "Sin datos";
        [ObservableProperty] private int overdueStartNotStarted;
        [ObservableProperty] private int measuredStarts;
        [ObservableProperty] private int measuredDueStarts;
        [ObservableProperty] private bool hasStartMetrics;
        [ObservableProperty] private bool hasOverdueStarts;

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

                FirstProgressAverageText = FormatFirstProgress(data.AvgHoursToFirstProgress);
                StartDelayAverageText = FormatStartDelay(data.AvgStartDelayDays);
                OnTimeStartRateText = data.MeasuredStarts == 0
                    ? "Sin datos"
                    : $"{data.OnTimeStartRate:P0}";
                LastMinuteStartRateText = data.MeasuredDueStarts == 0
                    ? "Sin datos"
                    : $"{data.LastMinuteStartRate:P0}";
                MeasuredStarts = data.MeasuredStarts;
                MeasuredDueStarts = data.MeasuredDueStarts;
                OverdueStartNotStarted = data.OverdueStartNotStarted;
                HasStartMetrics = data.MeasuredStarts > 0 || data.MeasuredDueStarts > 0;
                HasOverdueStarts = data.OverdueStartNotStarted > 0;

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

        private static string FormatFirstProgress(double? hours)
        {
            if (!hours.HasValue)
                return "Sin datos";

            return hours.Value < 24
                ? $"{hours.Value:F1} h"
                : $"{hours.Value / 24:F1} días";
        }

        private static string FormatStartDelay(double? days)
        {
            if (!days.HasValue)
                return "Sin datos";

            var rounded = Math.Round(days.Value, 1);
            if (Math.Abs(rounded) < 0.1)
                return "A tiempo";

            return rounded > 0
                ? $"+{rounded:F1} días"
                : $"{rounded:F1} días";
        }
    }
}
