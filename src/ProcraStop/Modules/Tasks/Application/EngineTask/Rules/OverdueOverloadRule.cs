
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Modules.Tasks.Application.EngineTask.Rules
{
    public class OverdueOverloadRule : IDecisionRule
    {

        public int Priority => 100;

        public bool IsMatch(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;

            return t.OverdueCount >= 3;
        }

        public DecisionResult CreateDecision(DecisionContext ctx)
        {
            var t = (TaskSignals)ctx.Tasks!;

            return new DecisionResult
            {
                Message = "Estás acumulando retrasos y mucha carga. Reduce el alcance hoy.",
                Type = DecisionType.Warning,
                Reason = $"Tienes {t.OverdueCount} tareas atrasadas y una carga estimada de {t.TotalEffort} minutos.",
                SuggestedAction = "No intentes hacer todo. Recupera control con una tarea pequeña.",
                Insights = new List<string>
                {
                    $"Tareas pendientes: {t.PendingCount}",
                    $"Tareas atrasadas: {t.OverdueCount}",
                    $"Carga total: {t.TotalEffort} min"
                }
            };
        }
    }
}
