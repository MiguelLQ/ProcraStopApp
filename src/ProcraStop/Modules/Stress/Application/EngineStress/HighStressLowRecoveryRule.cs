
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    internal class HighStressLowRecoveryRule : IDecisionRule
    {
        public int Priority => 90;

        public bool IsMatch(DecisionContext ctx)
        {
            var s = ctx.Stress;

            return s.CurrentLevel > 0.7 &&
                   s.RecoveryCapacity < 0.4;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var s = ctx.Stress;

            return new DecisionResult
            {
                Message = "Tu recuperación es insuficiente",
                Type = DecisionType.Warning,
                Reason = "Tu nivel de estrés es alto y no estás recuperándote",
                SuggestedAction = "Introduce pausas reales o reduce intensidad de hábitos",
                Insights = new List<string>
                    {
                        $"Stress: {s.CurrentLevel:F2}",
                        $"Recovery: {s.RecoveryCapacity:F2}"
                    }
            };
        }
    }
}
