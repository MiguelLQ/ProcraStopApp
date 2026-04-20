

using ProcraStop.Modules.Tasks.Application.DecisionEngine.Context;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class AvoidingHardTasksRule : IDecisionRule
    {
        // esta regla se activa cuando el usuario tiene varias tareas difíciles y
        // rápidas pendientes, pero no tiene tareas muy atrasadas,
        public int Priority => 70;
        // El nombre de esta regla es "AvoidingHardTasksRule" porque se enfoca en
        // identificar cuando el usuario podría estar evitando tareas difíciles,
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks;

            return t.HeavyTasksCount >= 3 &&
                   t.QuickTasksCount >= 2 &&
                   !t.HighOverdue;
        }
        // El mensaje de esta regla es "Podrías estar evitando tareas importantes."
        // porque se enfoca en alertar al usuario sobre el hecho de que podría estar procrastinando tareas difíciles,
        public DashboardDecisionDto CreateDecision(DecisionContext ctx)
        {
            return new DashboardDecisionDto
            {
                Message = "Podrías estar evitando tareas importantes.",
                Type = GuidanceType.Warning,
                SuggestedAction = "Avanza al menos 10 minutos en una tarea difícil"
            };
        }

    }
}
