using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class BurnoutRiskRule : IDecisionRule
    {
        public int Priority => 95;
        public DecisionCategory Category => DecisionCategory.Risk;
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks;
            var h = ctx.Habits;

            if (t is null || h is null) return false;

            return
                t.TotalEffort >= 240
                && h.ConsistencyScore < 0.5
                && h.FailureRate >= 0.25
                && h.WeeklyCompletionRate <= h.ConsistencyScore;
        }
        public DecisionResult CreateDecision(DecisionContext ctx, IReadOnlyList<UnifiedSignal> signals)
        {
            var t = (TaskSignals)ctx.Tasks!;
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás en riesgo de saturarte.",
                Type = DecisionType.Critical,
                Category = Category,
                Score = 0.95,
                Reason = $"Alta carga ({t.TotalEffort} min) combinada con bajo rendimiento ({h.ConsistencyScore * 100:0}%) y fallos ({h.FailureRate * 100:0}%).",

                SuggestedAction = "Reduce la carga hoy. Prioriza 1 tarea clave o descansa activamente.",

                Insights = new List<string>
                {
                    $"Carga total: {t.TotalEffort} min",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%",
                    $"Weekly: {h.WeeklyCompletionRate * 100:0}%",
                    $"Fallos: {h.FailureRate * 100:0}%"
                }
            };
        }
    }
}
