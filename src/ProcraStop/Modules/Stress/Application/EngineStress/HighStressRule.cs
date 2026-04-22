
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    public class HighStressRule :IDecisionRule
    {
        public int Priority => 90;

        public bool IsMatch(DecisionContext context)
        {
            return context.Stress.CurrentLevel > 0.7;
        }

        public DecisionResult CreateDecision(DecisionContext context)
        {
            return new DecisionResult
            {
                Message = "Nivel de estrés elevado",
                Type = DecisionType.Warning, // IMPORTANTE
                Reason = "Tu carga mental es alta actualmente",
                SuggestedAction = "Prioriza tareas simples o toma descansos",
                Insights = new List<string>
                {
                    $"Stress: {context.Stress.CurrentLevel:F2}",
                    $"Recovery: {context.Stress.RecoveryCapacity:F2}"
                }
            };
        }
    }
}
