

namespace ProcraStop.Core.Decisions.Signals
{
    public sealed class HabitSignals
    {
        // CORE SIGNALS (para engine)
        public double ConsistencyScore { get; init; }       // 0–1 (actualmente 7d, futuro 30d)
        public double WeeklyCompletionRate { get; init; }   // 0–1 (últimos 7 días)
        public double AverageEffort { get; init; }          // 0–1 (ponderado por weight)

        //  BEHAVIOR SIGNALS
        public int CurrentStreak { get; init; }             // streak global (mínimo)
        public int AverageStreak { get; init; }             // promedio por hábito
        public int BestStreak { get; init; }                // mejor hábito
        // fallos 
        public double FailureRate { get; init; }      // 0–1
        public double FailureDays { get; init; }      // ponderado
        public int FailureStreak { get; init; }       // días consecutivos fallando
        public static HabitSignals Empty()
        {
            return new HabitSignals
            {
                ConsistencyScore = 0,
                WeeklyCompletionRate = 0,
                AverageEffort = 0,
                CurrentStreak = 0,
                AverageStreak = 0,
                BestStreak = 0,
                FailureRate = 0,
                FailureDays = 0,
                FailureStreak = 0
            };
        }
    }
}
