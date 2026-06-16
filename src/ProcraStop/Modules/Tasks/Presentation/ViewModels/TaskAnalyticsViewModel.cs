

using CommunityToolkit.Mvvm.ComponentModel;
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
        [ObservableProperty] private int overdue;

        [ObservableProperty] private int highPriority;
        [ObservableProperty] private int mediumPriority;
        [ObservableProperty] private int lowPriority;

        public async Task LoadAsync()
        {
            var data = await _service.GetAsync(CancellationToken.None);
            Total = data.Total;
            Completed = data.Completed;
            Pending = data.Pending;
            Overdue = data.Overdue;

            HighPriority = data.HighPriority;
            MediumPriority = data.MediumPriority;
            LowPriority = data.LowPriority;
        }
    }
}
