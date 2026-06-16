
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Habits.EngineHabits.Rules
{
    public class HighFailureRule : IDecisionRule
    {
        public int Priority => 100; // más alta que todas
        public DecisionCategory Category => DecisionCategory.Risk;
        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;
            if (h is null) return false;

            return
                (
                    h.FailureRate >= 0.5
                    && h.FailureStreak >= 2
                )
                ||
                (
                    h.FailureStreak >= 3
                )
                ||
                (
                    h.WeeklyCompletionRate < 0.4
                    && h.ConsistencyScore < 0.5
                );
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás perdiendo el control de tus hábitos.",
                Type = DecisionType.Critical,
                Category = Category,
                Score = 0.95,
                Reason = BuildReason(h),

                SuggestedAction = "Reduce la carga inmediatamente. Elige 1 hábito clave y cúmplelo hoy.",

                Insights = new List<string>
                {
                    $"Fallos: {h.FailureRate * 100:0}%",
                    $"Racha de fallos: {h.FailureStreak} días",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%",
                    $"Weekly: {h.WeeklyCompletionRate * 100:0}%"
                }
            };
        }

        private static string BuildReason(HabitSignals h)
        {
            if (h.FailureStreak >= 3)
                return $"Llevas {h.FailureStreak} días consecutivos fallando.";

            if (h.FailureRate >= 0.5)
                return $"Más del {h.FailureRate * 100:0}% de tus hábitos están fallando.";

            return $"Bajo rendimiento reciente ({h.WeeklyCompletionRate * 100:0}%) con consistencia baja.";
        }
    }
}
