

using ProcraStop.Application.Habits.DTOs;
using ProcraStop.Application.Stress.DTOs;
using ProcraStop.Domain.Stress.Entities;
using ProcraStop.Domain.Stress.Enums;

namespace ProcraStop.Application.Stress.Services
{
    public class StressMetricsService
    {
        public StressSignals Calculate(
            TaskSignals tasks,
            HabitSignals habits,
            StressRecord? latest, // ya no lo usaremos directamente
            IEnumerable<StressRecord> history)
        {
            // 1. separar manual vs system
            var latestManual = history
            .Where(x => x.Source == StressSource.Manual &&
                        x.Timestamp.Date == DateTime.UtcNow.Date)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefault();

            var manual = latestManual?.Level;

            //  2. calcular stress del sistema
            var system = CalculateSystemStress(tasks, habits);

            //  3. combinar (modelo híbrido real)
            double finalStress;

            if (manual is not null)
            {
                finalStress = (0.6 * manual.Value) + (0.4 * system);
            }
            else
            {
                finalStress = system;
            }

            finalStress = Math.Clamp(finalStress, 0, 1);

            return new StressSignals
            {
                CurrentLevel = finalStress,
                Trend = CalculateTrend(history),
                RecoveryCapacity = CalculateRecovery(history),
                LoadTolerance = 1 - finalStress
            };
        }

        // =========================
        // TREND
        // =========================
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

        // =========================
        // RECOVERY
        // =========================
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

        // =========================
        // SYSTEM STRESS
        // =========================
        private double CalculateSystemStress(TaskSignals tasks, HabitSignals habits)
        {
            var taskLoad = NormalizeTasks(tasks);
            var habitInstability = 1 - habits.ConsistencyScore;

            return Math.Clamp(
                (0.7 * taskLoad) + (0.3 * habitInstability),
                0, 1
            );
        }

        // =========================
        // TASK NORMALIZATION
        // =========================
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
