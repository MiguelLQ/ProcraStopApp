
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class QuickWinRule : IDecisionRule
    {
        // Esta regla se activa cuando el usuario tiene tareas atrasadas pero también tiene
        // varias tareas rápidas pendientes
        public int Priority => 50;
        // Verifica si el usuario tiene tareas atrasadas y tareas rápidas pendientes
        public bool IsMatch(TaskSignals s)
        {
            return s.HasOverdueTasks && s.HasQuickTasks;
        }
        // Si la regla se activa, se sugiere al usuario que haga una tarea rápida para ganar impulso
        public DashboardDecisionDto CreateDecision(TaskSignals s)
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
