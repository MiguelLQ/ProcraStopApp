

using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Stress.Application.DTOs;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    public class HighStressRisingRule : IDecisionRule
    {
        public int Priority => 100;

        public bool IsMatch(DecisionContext ctx)
        {
            var s = ctx.Stress;

            return s.CurrentLevel > 0.75 &&
                   s.Trend == StressTrend.Rising;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var s = ctx.Stress;

            return new DecisionResult
            {
                Message = "Tu estrés está aumentando rápidamente",
                Type = DecisionType.Critical,
                Reason = "El nivel es alto y sigue en subida",
                SuggestedAction = "Reduce carga inmediatamente. Prioriza descanso o tareas ligeras",
                Insights = new List<string>
                {
                    $"Stress: {s.CurrentLevel:F2}",
                    $"Trend: {s.Trend}",
                    $"Recovery: {s.RecoveryCapacity:F2}"
                }
            };
        }
    }
}
