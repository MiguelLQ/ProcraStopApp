using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Modules.Habits.Application.EngineHabits.Rules
{
    public class LowConsistencyRule : IDecisionRule
    {
        public int Priority => 90;
        public DecisionCategory Category => DecisionCategory.Risk;
        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;

            if (h is null) return false;

            return h.ConsistencyScore < 0.5
                && h.WeeklyCompletionRate < 0.6
                && h.FailureRate >= 0.3;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Tu consistencia está cayendo.",
                Type = DecisionType.Warning,
                Category = Category,
                Score = 0.75,
                Reason = $"Consistencia baja ({h.ConsistencyScore * 100:0}%) con un nivel de fallos de {h.FailureRate * 100:0}%.",
                SuggestedAction = "Reduce la dificultad y enfócate en cumplir lo mínimo diario.",
                Insights = new List<string>
            {
                $"Consistencia: {h.ConsistencyScore * 100:0}%",
                $"Cumplimiento semanal: {h.WeeklyCompletionRate * 100:0}%",
                $"Fallos: {h.FailureRate * 100:0}%",
                $"Streak actual: {h.CurrentStreak} días"
            }
            };
        }
    }
}
