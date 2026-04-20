
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
        public bool IsMatch(TaskSignals s)
        {
            return s.HighOverdue && s.HighTotalEffort;
        }
        // Si la regla se activa, se sugiere al usuario que reduzca el alcance de su trabajo para evitar sentirse abrumado
        public DashboardDecisionDto CreateDecision(TaskSignals s)
        {
            return new DashboardDecisionDto
            {
                Message = "Estás acumulando retrasos y mucha carga. Reduce el alcance hoy.",
                Type = GuidanceType.Warning,
                SuggestedAction = "Elige una tarea pequeña y avanza solo en eso"
            };
        }
    }
}
