
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Stress.Application.DTOs;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    public class RecoveryNeededRule : IDecisionRule
    {
        public int Priority => 95;

        public bool IsMatch(DecisionContext context)
        {
            return context.Stress.CurrentLevel > 0.6 &&
                   context.Stress.Trend == StressTrend.Rising;
        }

        public DecisionResult CreateDecision(DecisionContext context)
        {
            return new DecisionResult
            {
                Message = "Tu estrés está aumentando",
                Type = DecisionType.Critical, //
                Reason = "El nivel de estrés muestra una tendencia ascendente",
                SuggestedAction = "Haz una pausa o reduce el ritmo antes de saturarte",
                Insights = new List<string>
                {
                    $"Trend: {context.Stress.Trend}",
                    $"Stress: {context.Stress.CurrentLevel:F2}"
                }
            };
        }
    }
}
