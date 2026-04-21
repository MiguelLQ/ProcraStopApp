

namespace ProcraStop.Modules.Habits.Application.DTOs
{
    public sealed class HabitSignals
    {
        public double ConsistencyScore { get; init; }           // 0 - 1
        public int CurrentStreak { get; init; }                 // días consecutivos cumplidos
        public double AverageEffort { get; init; }              // promedio relativo (0 - 1)
        public double WeeklyCompletionRate { get; init; }       // cumplimiento semanal (0 - 1)
    }
}
