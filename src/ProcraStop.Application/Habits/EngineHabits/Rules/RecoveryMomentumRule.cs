using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Application.Habits.EngineHabits.Rules
{
    public class RecoveryMomentumRule : IDecisionRule
    {
        public int Priority => 70;
        public DecisionCategory Category => DecisionCategory.Recovery;
        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;
            if (h is null) return false;

            return h.ConsistencyScore >= 0.4
                && h.ConsistencyScore < 0.75
                && h.WeeklyCompletionRate > h.ConsistencyScore
                && h.CurrentStreak >= 2
                && h.FailureRate < 0.4;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás recuperando el ritmo.",
                Type = DecisionType.Info,
                Category = Category,
                Score = 0.65,
                Reason = $"Tu rendimiento reciente está mejorando ({h.WeeklyCompletionRate * 100:0}%) frente a tu consistencia general ({h.ConsistencyScore * 100:0}%).",
                SuggestedAction = "Mantén estabilidad. No aumentes carga todavía.",
                Insights = new List<string>
            {
                $"Streak actual: {h.CurrentStreak} días",
                $"Consistencia: {h.ConsistencyScore * 100:0}%",
                $"Weekly: {h.WeeklyCompletionRate * 100:0}%",
                $"Fallos: {h.FailureRate * 100:0}%"
            }
            };
        }
    }
}
