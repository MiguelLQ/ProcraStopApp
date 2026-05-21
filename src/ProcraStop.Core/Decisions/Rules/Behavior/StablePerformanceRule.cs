
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class StablePerformanceRule : IDecisionRule
    {
        public int Priority => 60;
        public DecisionCategory Category => DecisionCategory.Performance;
        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;
            var s = ctx.Stress;

            if (h is null || s is null) return false;

            return
                h.ConsistencyScore >= 0.7
                && h.FailureRate < 0.15
                && h.WeeklyCompletionRate >= h.ConsistencyScore
                && s.CurrentLevel < 0.5;
        }
        public DecisionResult CreateDecision(
            DecisionContext ctx,
            IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;
            var s = ctx.Stress!;

            var stressSignal = signals
                .FirstOrDefault(x => x.Category == "Stress");

            var adjustedStress = stressSignal?.Score ?? s.CurrentLevel;

            return new DecisionResult
            {
                Message = "Estás manteniendo un ritmo estable y saludable.",
                Type = DecisionType.Success,
                Category = Category,
                Score = 0.8,

                Reason = $"Consistencia {h.ConsistencyScore * 100:0}% con stress ajustado {adjustedStress * 100:0}%",

                SuggestedAction = "Mantén esta rutina.",

                Insights = new List<string>
        {
            $"Consistency: {h.ConsistencyScore * 100:0}%",
            $"Stress(signal): {adjustedStress * 100:0}%"
        }
            };
        }
    }
}
