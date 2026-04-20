

using ProcraStop.Modules.Tasks.Application.DecisionEngine.DTOs;
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals;
using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Rules
{
    public interface IDecisionRule
    {
        // La prioridad de la regla, donde un número menor indica una mayor prioridad
        int Priority { get; }
        // Método para verificar si la regla coincide con los indicadores de la tarea
        bool IsMatch(TaskSignals signals);
        // Método para crear una decisión basada en los indicadores de la tarea
        DashboardDecisionDto CreateDecision(TaskSignals signals);
    }
}
