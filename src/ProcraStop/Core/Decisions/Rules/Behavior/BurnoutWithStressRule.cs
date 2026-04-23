
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class BurnoutWithStressRule : IDecisionRule
    {
        public int Priority => 110; // MÁS alta que la original

        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks!;
            var h = ctx.Habits;
            var s = ctx.Stress;

            if (h is null) return false;

            return t.TotalEffort >= 240 &&
                   h.ConsistencyScore < 0.4 &&
                   s.CurrentLevel > 0.7;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = ctx.Tasks!;
            var h = ctx.Habits!;
            var s = ctx.Stress;

            return new DecisionResult
            {
                Message = "Riesgo crítico de burnout detectado",
                Type = DecisionType.Critical,
                Reason = "Alta carga, baja consistencia y estrés elevado",
                SuggestedAction = "Reduce carga inmediatamente. Prioriza descanso real",
                Insights = new List<string>
                {
                    $"Carga: {t.TotalEffort} min",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%",
                    $"Stress: {s.CurrentLevel:F2}"
                }
            };
        }
    }
}
