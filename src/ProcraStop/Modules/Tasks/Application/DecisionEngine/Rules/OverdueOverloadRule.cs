
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Context;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class OverdueOverloadRule : IDecisionRule
    {
        // Esta regla se activa cuando el usuario tiene más de 3 tareas atrasadas
        public int Priority => 100;
        // La regla se activa si el usuario tiene una alta cantidad de tareas atrasadas y una alta carga total de trabajo
        public bool IsMatch(DecisionContext ctx)
        {
            var t = ctx.Tasks;

            return t.OverdueCount >= 3;
        }
        // Si la regla se activa, se sugiere al usuario que reduzca el alcance de su trabajo para evitar sentirse abrumado
        public DashboardDecisionDto CreateDecision(DecisionContext ctx)
        {
            var t = ctx.Tasks;

            return new DashboardDecisionDto
            {
                Message = "Estás acumulando retrasos y mucha carga. Reduce el alcance hoy.",
                Type = GuidanceType.Warning,
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
