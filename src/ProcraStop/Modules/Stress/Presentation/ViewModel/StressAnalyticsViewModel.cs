
using CommunityToolkit.Mvvm.ComponentModel;
using ProcraStop.Modules.Stress.Application.DTOs;
using ProcraStop.Modules.Stress.Application.Queries.GetStressAnalytics;
using System.Globalization;

namespace ProcraStop.Modules.Stress.Presentation.ViewModel
{
    public partial class StressAnalyticsViewModel : ObservableObject
    {
        private readonly GetStressAnalyticsHandler _handler;

        public StressAnalyticsViewModel(GetStressAnalyticsHandler handler)
        {
            _handler = handler;
        }

        // ── KPIs ────────────────────────────────────────────────
        [ObservableProperty] private double average;
        [ObservableProperty] private double max;
        [ObservableProperty] private double min;
        [ObservableProperty] private double recoveryScore;
        [ObservableProperty] private int highStressDays;
        [ObservableProperty] private double highStressRatio;

        // ── Trend badge ──────────────────────────────────────────
        [ObservableProperty] private string trendText = string.Empty;
        [ObservableProperty] private Color trendBadgeColor = Colors.Transparent;
        [ObservableProperty] private Color trendTextColor = Colors.Gray;

        // ── Mejor día ────────────────────────────────────────────
        [ObservableProperty] private string bestDayLabel = string.Empty;
        [ObservableProperty] private double bestDayValue;

        // ── Chart ────────────────────────────────────────────────
        [ObservableProperty] private double[] chartValues = Array.Empty<double>();
        [ObservableProperty] private string[] chartLabels = Array.Empty<string>();

        // ────────────────────────────────────────────────────────
        public async Task LoadAsync()
        {
            try
            {
                var result = await _handler.Handle(new GetStressAnalyticsQuery { Days = 7 });

                // KPIs
                Average = result.Average;
                Max = result.Max;
                Min = result.Min;
                RecoveryScore = result.RecoveryScore;
                HighStressDays = result.HighStressDays;

                // Chart
                if (result.Points?.Any() == true)
                {
                    ChartValues = result.Points.Select(p => p.Level).ToArray();
                    ChartLabels = result.Points.Select(p => p.Date.ToString("ddd")).ToArray();
                }

                // Extras (dependen de los valores ya asignados arriba)
                ApplyExtras(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERROR: {ex}");
            }
        }

        private void ApplyExtras(StressAnalyticsDto result)
        {
            // Ratio para la ProgressBar (0.0 – 1.0)
            HighStressRatio = HighStressDays / 7.0;

            // Mejor día (el de menor nivel)
            if (result.Points?.Any() == true)
            {
                var best = result.Points.OrderBy(p => p.Level).First();
                BestDayLabel = best.Date.ToString("dddd", new CultureInfo("es-ES"));
                // Capitalize primera letra
                BestDayLabel = char.ToUpper(BestDayLabel[0]) + BestDayLabel[1..];
                BestDayValue = best.Level;
            }

            // Badge tendencia
            var trendStr = result.Trend.ToString();
            bool isUp = trendStr.Contains("Up", StringComparison.OrdinalIgnoreCase)
                     || trendStr.Contains("Alta", StringComparison.OrdinalIgnoreCase)
                     || trendStr.Contains("Aumenta", StringComparison.OrdinalIgnoreCase);

            TrendBadgeColor = isUp
                ? Color.FromArgb("#FCEBEB")
                : Color.FromArgb("#EAF3DE");

            TrendTextColor = isUp
                ? Color.FromArgb("#A32D2D")
                : Color.FromArgb("#3B6D11");

            TrendText = isUp ? "↑ Tendencia alta" : "↓ Tendencia baja";
        }
    }
}

