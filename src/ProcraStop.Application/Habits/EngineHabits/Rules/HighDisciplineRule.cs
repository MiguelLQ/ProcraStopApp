

using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Habits.EngineHabits.Rules
{
    public class HighDisciplineRule : IDecisionRule
    {
        public int Priority => 40;
        public DecisionCategory Category => DecisionCategory.Performance;

        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;

            if (h is null) return false;

            return h.ConsistencyScore >= 0.8
                && h.CurrentStreak >= 3
                && h.FailureRate < 0.2
                && h.FailureStreak <= 1;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás siendo muy consistente. Buen trabajo.",
                Type = DecisionType.Success,
                Category = Category,
                Score = 0.9,
                Reason = $"Alta consistencia ({h.ConsistencyScore * 100:0}%) con bajo nivel de fallos ({h.FailureRate * 100:0}%).",
                SuggestedAction = "Aprovecha este momento para avanzar en tareas importantes.",
                Insights = new List<string>
                {
                    $"Streak: {h.CurrentStreak} días",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%"
                }
            };
        }
    }
}
