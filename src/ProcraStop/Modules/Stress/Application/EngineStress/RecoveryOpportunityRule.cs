

using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Stress.Application.DTOs;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    public class RecoveryOpportunityRule :IDecisionRule
    {
        public int Priority => 70;

        public bool IsMatch(DecisionContext ctx)
        {
            var s = ctx.Stress;

            return s.CurrentLevel > 0.6 &&
                   s.Trend == StressTrend.Falling;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var s = ctx.Stress;

            return new DecisionResult
            {
                Message = "Estás mejorando, mantén el ritmo",
                Type = DecisionType.Info,
                Reason = "Tu estrés sigue bajando",
                SuggestedAction = "Mantén hábitos actuales y evita sobrecargarte de nuevo",
                Insights = new List<string>
            {
                $"Stress: {s.CurrentLevel:F2}",
                $"Trend: {s.Trend}"
            }
            };
        }
    }
}
