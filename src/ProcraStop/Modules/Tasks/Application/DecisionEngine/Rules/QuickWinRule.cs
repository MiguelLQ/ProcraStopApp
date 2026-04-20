
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Context;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class QuickWinRule : IDecisionRule
    {
        // Esta regla se activa cuando el usuario tiene tareas atrasadas pero también tiene
        // varias tareas rápidas pendientes
        public int Priority => 50;
        // La regla se activa si el usuario tiene tareas de alta prioridad atrasadas
        // pero también tiene tareas de alta prioridad con poco esfuerzo total
        public bool IsMatch(DecisionContext ctx)
        {
            return ctx.Tasks.HighOverdue && ctx.Tasks.HighTotalEffort;
        }
        // Si la regla se activa, se sugiere al usuario que aproveche las tareas rápidas para ganar impulso
        public DashboardDecisionDto CreateDecision(DecisionContext  ctx)
        {
            return new DashboardDecisionDto
            {
                Message = "Tienes tareas atrasadas pero fáciles.",
                Type = GuidanceType.Info,
                SuggestedAction = "Haz una tarea rápida ahora"
            };
        }
    }
}
