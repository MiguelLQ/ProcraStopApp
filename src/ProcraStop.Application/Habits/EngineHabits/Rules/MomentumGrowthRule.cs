using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Habits.EngineHabits.Rules
{
    public class MomentumGrowthRule : IDecisionRule
    {
        public int Priority => 65;
        public DecisionCategory Category => DecisionCategory.Recovery;
        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;

            if (h is null) return false;

            return
                h.WeeklyCompletionRate > h.ConsistencyScore
                && h.CurrentStreak >= 2
                && h.FailureRate < 0.3;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás mejorando progresivamente.",
                Type = DecisionType.Success,
                Category = Category,
                Score = 0.7,
                Reason = $"Tu rendimiento reciente ({h.WeeklyCompletionRate * 100:0}%) supera tu promedio ({h.ConsistencyScore * 100:0}%).",

                SuggestedAction = "Sigue así. Consolidar es más importante que acelerar.",

                Insights = new List<string>
            {
                $"Weekly: {h.WeeklyCompletionRate * 100:0}%",
                $"Consistencia: {h.ConsistencyScore * 100:0}%",
                $"Streak: {h.CurrentStreak}"
            }
            };
        }
    }
}
