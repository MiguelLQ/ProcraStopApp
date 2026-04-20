
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Context;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class OverPlanningRule : IDecisionRule
    {
        // esta regla se activa cuando el usuario tiene muchas tareas pendientes, con un alto esfuerzo total,
        // pero no tiene tareas rápidas. Esto sugiere que el usuario podría estar sobreplanificando y
        // necesita reducir su lista de tareas para enfocarse en lo más importante.
        public int Priority => 80;
        // El nombre de esta regla es "OverPlanningRule" porque se enfoca en identificar cuando el
        // usuario tiene demasiadas tareas planificadas,
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks;

            return t.PendingCount >= 8 && t.TotalEffort >= 240;
        }
        // El mensaje de esta regla es "Tienes demasiadas tareas planificadas." porque se enfoca en
        // alertar al usuario sobre el hecho de que tiene una cantidad excesiva de tareas pendientes,
        public DashboardDecisionDto CreateDecision(DecisionContext ctx)
        {
            return new DashboardDecisionDto
            {
                Message = "Tienes demasiadas tareas planificadas.",
                Type = GuidanceType.Warning,
                SuggestedAction = "Reduce tu lista y enfócate en 1–2 tareas clave"
            };
        }
    }
}
