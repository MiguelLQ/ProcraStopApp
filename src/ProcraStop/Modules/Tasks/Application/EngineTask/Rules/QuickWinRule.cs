using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Tasks.Application.EngineTask.Rules
{
    public class QuickWinRule : IDecisionRule
    {
        public int Priority => 50;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.TaskSignals!;

            return t.OverdueCount >= 1 && t.QuickTasksCount >= 2;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.TaskSignals!;

            return new DecisionResult
            {
                Message = "Tienes tareas atrasadas pero fáciles.",
                Type = DecisionType.Info,
                Reason = $"Tienes {t.OverdueCount} tareas atrasadas y {t.QuickTasksCount} tareas rápidas disponibles.",
                SuggestedAction = "Haz una tarea rápida ahora para recuperar control.",
                Insights = new List<string>
                {
                    $"Atrasadas: {t.OverdueCount}",
                    $"Rápidas: {t.QuickTasksCount}"
                }
            };
        }
    }
}
