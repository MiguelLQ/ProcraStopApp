using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class LowStressHighEfficiencyRule : IDecisionRule
    {
        public int Priority => 75;
        public DecisionCategory Category => DecisionCategory.Performance;
        public bool IsMatch(DecisionContext ctx)
        {
            var h = ctx.Habits;
            var s = ctx.Stress;

            if (h is null || s is null) return false;

            return
                h.ConsistencyScore >= 0.65
                && h.FailureRate < 0.2
                && s.CurrentLevel < 0.4;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var h = ctx.Habits!;
            var s = ctx.Stress!;

            return new DecisionResult
            {
                Message = "Estás siendo eficiente sin generar presión.",
                Type = DecisionType.Success,
                Category = Category,
                Score = 0.85,
                Reason = $"Buen rendimiento con bajo nivel de estrés ({s.CurrentLevel * 100:0}%).",

                SuggestedAction = "Este es un estado ideal. Mantén equilibrio.",

                Insights = new List<string>
            {
                $"Consistencia: {h.ConsistencyScore * 100:0}%",
                $"Fallos: {h.FailureRate * 100:0}%",
                $"Stress: {s.CurrentLevel * 100:0}%"
            }
            };
        }
    }
}
