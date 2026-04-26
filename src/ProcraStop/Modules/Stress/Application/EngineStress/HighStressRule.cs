
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    public class HighStressRule :IDecisionRule
    {
        public int Priority => 90;
        public DecisionCategory Category => DecisionCategory.Stress;
        public bool IsMatch(DecisionContext context)
        {
            return context.Stress.CurrentLevel > 0.7;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            return new DecisionResult
            {
                Message = "Nivel de estrés elevado",
                Type = DecisionType.Warning, // IMPORTANTE
                Category = Category,
                Score = 0.8,
                Reason = "Tu carga mental es alta actualmente",
                SuggestedAction = "Prioriza tareas simples o toma descansos",
                Insights = new List<string>
                {
                    $"Stress: {ctx.Stress.CurrentLevel:F2}",
                    $"Recovery: {ctx.Stress.RecoveryCapacity:F2}"
                }
            };
        }
    }
}
