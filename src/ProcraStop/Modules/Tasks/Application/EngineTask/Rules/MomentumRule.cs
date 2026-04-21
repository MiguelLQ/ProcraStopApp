using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;
namespace ProcraStop.Modules.Tasks.Application.EngineTask.Rules
{
    public class MomentumRule : IDecisionRule
    {
        public int Priority => 60;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.TaskSignals!;

            return t.PendingCount >= 2 &&
                   t.QuickTasksCount >= 2 &&
                   !t.HasOverdueTasks;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.TaskSignals!;

            return new DecisionResult
            {
                Message = "Te conviene generar impulso.",
                Type = DecisionType.Info,
                Reason = $"Tienes {t.QuickTasksCount} tareas rápidas disponibles y ninguna atrasada.",
                SuggestedAction = "Empieza con una tarea corta para entrar en ritmo",
                Insights = new List<string>
                {
                    $"Tareas rápidas: {t.QuickTasksCount}",
                    $"Tareas pendientes: {t.PendingCount}"
                }
            };
        }
    }
}
