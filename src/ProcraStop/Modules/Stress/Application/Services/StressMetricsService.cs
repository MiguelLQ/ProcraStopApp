
using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Stress.Application.DTOs;
using ProcraStop.Modules.Stress.Domain.Entities;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Stress.Application.Services
{
    public class StressMetricsService
    {
        public StressSignals Calculate(
       TaskSignals tasks,
       HabitSignals habits,
       StressRecord? latest,
       IEnumerable<StressRecord> history)
        {
            var manual = latest?.Level ?? 0.5;

            var sTasks = NormalizeTasks(tasks);
            var sHabits = 1 - habits.ConsistencyScore;

            var stress = Math.Clamp(
                (0.4 * manual) +
                (0.4 * sTasks) +
                (0.2 * sHabits),
                0, 1
            );

            return new StressSignals
            {
                CurrentLevel = stress,
                Trend = CalculateTrend(history),
                RecoveryCapacity = CalculateRecovery(history),
                LoadTolerance = 1 - stress
            };
        }
        private StressTrend CalculateTrend(IEnumerable<StressRecord> history)
        {
            var values = history
                .OrderBy(x => x.Timestamp)
                .TakeLast(5)
                .Select(x => x.Level)
                .ToList();

            if (values.Count < 2) return StressTrend.Stable;

            var diff = values.Last() - values.First();

            if (diff > 0.05) return StressTrend.Rising;
            if (diff < -0.05) return StressTrend.Falling;

            return StressTrend.Stable;
        }
        private double CalculateRecovery(IEnumerable<StressRecord> history)
        {
            var recent = history
                .OrderByDescending(x => x.Timestamp)
                .Take(5)
                .ToList();

            if (!recent.Any()) return 1;

            var avg = recent.Average(x => x.Level);

            var trend = CalculateTrend(history);

            var recovery = 1 - avg;

            if (trend == StressTrend.Rising)
                recovery *= 0.7;

            if (trend == StressTrend.Falling)
                recovery *= 1.2;

            return Math.Clamp(recovery, 0, 1);
        }
        private double NormalizeTasks(TaskSignals t)
        {
            var overdueFactor = Math.Min(t.OverdueCount / 5.0, 1);
            var pendingFactor = Math.Min(t.PendingCount / 10.0, 1);
            var effortFactor = Math.Min(t.TotalEffort / 300.0, 1);

            return (0.5 * overdueFactor) +
                   (0.2 * pendingFactor) +
                   (0.3 * effortFactor);
        }
    }

}
