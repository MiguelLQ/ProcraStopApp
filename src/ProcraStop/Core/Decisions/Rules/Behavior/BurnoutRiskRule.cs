using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Rules.Behavior
{
    public class BurnoutRiskRule : IDecisionRule
    {
        public int Priority => 95;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.TaskSignals!;
            var h = ctx.Habits;

            if (h is null) return false;

            return t.TotalEffort >= 240 && h.ConsistencyScore < 0.4;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.TaskSignals!;
            var h = ctx.Habits!;

            return new DecisionResult
            {
                Message = "Estás en riesgo de saturarte.",
                Type = DecisionType.Critical,
                Reason = $"Alta carga ({t.TotalEffort} min) y baja consistencia ({h.ConsistencyScore * 100:0}%).",
                SuggestedAction = "Reduce tu carga hoy y prioriza solo una tarea clave.",
                Insights = new List<string>
                {
                    $"Carga total: {t.TotalEffort} min",
                    $"Consistencia: {h.ConsistencyScore * 100:0}%"
                }
            };
        }
    }
}
