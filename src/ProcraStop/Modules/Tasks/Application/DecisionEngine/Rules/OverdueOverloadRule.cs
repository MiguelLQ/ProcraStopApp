
using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public class OverdueOverloadRule : IDecisionRule
    {
        // Esta regla se activa cuando el usuario tiene más de 3 tareas atrasadas
        public int Priority => 100;
        // el siguiente paso es evaluar si el usuario tiene una carga de trabajo excesiva,
        // considerando tanto el número de tareas atrasadas como el esfuerzo estimado total
        public bool IsMatch(IReadOnlyList<StudyTask> tasks)
        {
            var pending = tasks.Where(t => !t.IsCompleted && !t.IsArchived).ToList();

            var overdueCount = pending.Count(t => t.IsOverdue);
            var totalEffort = pending.Sum(t => t.EstimatedMinutes ?? 30);

            return overdueCount >= 3 && totalEffort > 300;
        }
        // Si la regla se activa, se sugiere al usuario reducir el alcance de su trabajo para evitar sentirse abrumado
        public DashboardDecisionDto CreateDecision(IReadOnlyList<StudyTask> tasks)
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
