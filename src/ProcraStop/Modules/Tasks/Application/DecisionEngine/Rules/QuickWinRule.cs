
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class QuickWinRule : IDecisionRule
    {
        // Esta regla se activa cuando el usuario tiene tareas atrasadas pero también tiene
        // varias tareas rápidas pendientes
        public int Priority => 50;
        // el siguiente paso es evaluar si el usuario tiene tareas atras    adas pero
        // también tiene varias tareas rápidas pendientes,
        public bool IsMatch(IReadOnlyList<StudyTask> tasks)
        {
            var pending = tasks.Where(t => !t.IsCompleted && !t.IsArchived).ToList();

            var overdueCount = pending.Count(t => t.IsOverdue);
            var quickTasks = pending.Count(t => t.GetEffort() == TaskEffort.Quick);

            return overdueCount > 0 && quickTasks >= 2;
        }
        // Si la regla se activa, se sugiere al usuario que haga una tarea rápida para ganar impulso
        public DashboardDecisionDto CreateDecision(IReadOnlyList<StudyTask> tasks)
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
