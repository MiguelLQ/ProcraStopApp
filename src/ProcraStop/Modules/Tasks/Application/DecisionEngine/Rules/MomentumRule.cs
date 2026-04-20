using ProcraStop.Modules.Tasks.Application.DecisionEngine.Context;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;
namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class MomentumRule : IDecisionRule
    {
        // esta regla se activa cuando el usuario tiene varias tareas pendientes pero no tiene tareas atrasadas,
        // lo que indica que está en un buen ritmo de trabajo. El mensaje sugiere que el usuario aproveche ese impulso para seguir avanzando,
        // comenzando con una tarea corta para mantener el ritmo.
        public int Priority => 60;
        // El nombre de esta regla es "MomentumRule" porque se enfoca en identificar cuando el usuario tiene un buen ritmo de trabajo (momentum)
        // y sugiere aprovecharlo para seguir avanzando.
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks;

            return t.PendingCount >= 2 &&
                   t.QuickTasksCount >= 2 &&
                   !t.HasOverdueTasks;
        }
        // El mensaje de esta regla es "Te conviene generar impulso." porque se enfoca en motivar al usuario a aprovechar el buen ritmo de trabajo que tiene,
        // sugiriendo que empiece con una tarea corta para mantener ese impulso.
        public DashboardDecisionDto CreateDecision(DecisionContext ctx)
        {
            var t = ctx.Tasks;
            return new DashboardDecisionDto
            {
                Message = "Te conviene generar impulso.",
                Type = GuidanceType.Info,
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
