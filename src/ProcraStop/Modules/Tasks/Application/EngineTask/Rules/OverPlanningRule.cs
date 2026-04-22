using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Tasks.Application.EngineTask.Rules
{
    public class OverPlanningRule : IDecisionRule
    {
        public int Priority => 80;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;

            return t.PendingCount >= 8 && t.TotalEffort >= 240;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;

            return new DecisionResult
            {
                Message = "Tienes demasiadas tareas planificadas.",
                Type = DecisionType.Warning,
                Reason = $"Tienes {t.PendingCount} tareas con una carga total de {t.TotalEffort} minutos.",
                SuggestedAction = "Reduce tu lista y enfócate en 1–2 tareas clave",
                Insights = new List<string>
                {
                    $"Tareas pendientes: {t.PendingCount}",
                    $"Carga total: {t.TotalEffort} min"
                }
            };
        }
    }
}
