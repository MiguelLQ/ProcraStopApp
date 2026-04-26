
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Stress.Application.DTOs;

namespace ProcraStop.Modules.Stress.Application.EngineStress
{
    public class RecoveryNeededRule : IDecisionRule
    {
        public int Priority => 95;
        public DecisionCategory Category => DecisionCategory.Stress;
        public bool IsMatch(DecisionContext context)
        {
            return context.Stress.CurrentLevel > 0.6 &&
                   context.Stress.Trend == StressTrend.Rising;
        }

        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            return new DecisionResult
            {
                Message = "Tu estrés está aumentando",
                Type = DecisionType.Warning,
                Category = Category,
                Score = 0.88,
                Reason = "El nivel de estrés muestra una tendencia ascendente",
                SuggestedAction = "Haz una pausa o reduce el ritmo antes de saturarte",
                Insights = new List<string>
                {
                    $"Trend: {ctx.Stress.Trend}",
                    $"Stress: {ctx.Stress.CurrentLevel:F2}"
                }
            };
        }
    }
}
