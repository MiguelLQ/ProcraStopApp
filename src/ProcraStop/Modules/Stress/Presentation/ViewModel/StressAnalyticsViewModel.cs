
using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Stress.Application.Queries.GetStressAnalytics;

namespace ProcraStop.Modules.Stress.Presentation.ViewModel
{
    public partial class StressAnalyticsViewModel : ObservableObject
    {
        private readonly GetStressAnalyticsHandler _handler;

        public StressAnalyticsViewModel(GetStressAnalyticsHandler handler)
        {
            _handler = handler;
        }

        [ObservableProperty]
        private double average;

        [ObservableProperty]
        private double max;

        [ObservableProperty]
        private double min;

        [ObservableProperty]
        private string trend = string.Empty;

        [ObservableProperty]
        private int highStressDays;

        [ObservableProperty]
        private double recoveryScore;

        public async Task LoadAsync()
        {
            var result = await _handler.Handle(new GetStressAnalyticsQuery
            {
                Days = 7
            });

            Average = result.Average;
            Max = result.Max;
            Min = result.Min;
            Trend = result.Trend.ToString();
            HighStressDays = result.HighStressDays;
            RecoveryScore = result.RecoveryScore;
        }
    }
}

