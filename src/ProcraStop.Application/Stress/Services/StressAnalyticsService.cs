
using ProcraStop.Application.Stress.DTOs;
using ProcraStop.Core.Decisions.Enums;
using ProcraStop.Domain.Stress.Entities;
namespace ProcraStop.Application.Stress.Services
{
    public class StressAnalyticsService
    {
        public StressAnalyticsDto Calculate(IEnumerable<StressRecord> data)
        {
            var list = data
                .OrderBy(x => x.Timestamp)
                .ToList();

            if (!list.Any())
                return new StressAnalyticsDto();

            var levels = list.Select(x => x.Level).ToList();

            return new StressAnalyticsDto
            {
                Average = levels.Average(),
                Max = levels.Max(),
                Min = levels.Min(),
                Trend = CalculateTrend(levels),
                HighStressDays = list.Count(x => x.Level > 0.7),
                RecoveryScore = CalculateRecovery(levels)
            };
        }

        public StressTrend CalculateTrend(List<double> values)
        {
            if (values.Count < 2)
                return StressTrend.Stable;

            var diff = values.Last() - values.First();

            if (diff > 0.05) return StressTrend.Rising;
            if (diff < -0.05) return StressTrend.Falling;

            return StressTrend.Stable;
        }

        private double CalculateRecovery(List<double> values)
        {
            var avg = values.Average();

            var recovery = 1 - avg;

            var trend = CalculateTrend(values);

            if (trend == StressTrend.Rising)
                recovery *= 0.8;

            if (trend == StressTrend.Falling)
                recovery *= 1.1;

            return Math.Clamp(recovery, 0, 1);
        }

    }
}
