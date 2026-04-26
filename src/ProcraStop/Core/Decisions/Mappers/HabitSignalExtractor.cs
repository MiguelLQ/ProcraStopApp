
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Habits.Application.DTOs;

namespace ProcraStop.Core.Decisions.Mappers
{
    public static class HabitSignalExtractor
    {
        public static IEnumerable<UnifiedSignal> Extract(HabitSignals habits)
        {
            var signals = new List<UnifiedSignal>();

            // 1. STABILITY
            signals.Add(new UnifiedSignal
            {
                Name = "Habit Consistency",
                Category = "Behavior",
                Score = habits.ConsistencyScore,
                Type = habits.ConsistencyScore > 0.7
                    ? DecisionType.Success
                    : DecisionType.Warning,
                Source = "Habit"
            });

            signals.Add(new UnifiedSignal
            {
                Name = "Weekly Completion",
                Category = "Behavior",
                Score = habits.WeeklyCompletionRate,
                Type = habits.WeeklyCompletionRate > 0.7
                    ? DecisionType.Success
                    : DecisionType.Info,
                Source = "Habit"
            });

            // 2. MOMENTUM
            signals.Add(new UnifiedSignal
            {
                Name = "Habit Momentum",
                Category = "Trajectory",
                Score = NormalizeStreak(habits.CurrentStreak),
                Type = habits.CurrentStreak >= habits.BestStreak * 0.8
                    ? DecisionType.Success
                    : DecisionType.Info,
                Source = "Habit"
            });

            // 3. DEGRADATION
            signals.Add(new UnifiedSignal
            {
                Name = "Failure Pressure",
                Category = "Risk",
                Score = habits.FailureRate,
                Type = habits.FailureRate > 0.5
                    ? DecisionType.Warning
                    : DecisionType.Info,
                Source = "Habit"
            });

            signals.Add(new UnifiedSignal
            {
                Name = "Failure Streak Risk",
                Category = "Risk",
                Score = NormalizeFailure(habits.FailureStreak),
                Type = habits.FailureStreak > 3
                    ? DecisionType.Critical
                    : DecisionType.Warning,
                Source = "Habit"
            });

            return signals;
        }

        private static double NormalizeStreak(int streak)
        {
            return Math.Min(1.0, streak / 30.0); // 30 días referencia
        }

        private static double NormalizeFailure(int failureStreak)
        {
            return Math.Min(1.0, failureStreak / 7.0); // 7 días crítico
        }
    }
}
