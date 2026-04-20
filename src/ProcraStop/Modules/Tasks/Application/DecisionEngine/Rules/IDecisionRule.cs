

using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public interface IDecisionRule
    {
        // La prioridad de la regla, donde un número menor indica una mayor prioridad
        int Priority { get; }
        // Método para evaluar si la regla se aplica a la lista de tareas dada
        bool IsMatch(IReadOnlyList<StudyTask> tasks);

        // Método para crear una decisión basada en la lista de tareas dada
        DashboardDecisionDto CreateDecision(IReadOnlyList<StudyTask> tasks);
    }
}
