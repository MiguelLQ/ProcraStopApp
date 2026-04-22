using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Context
{
    public class DecisionContext
    {
        // Indicadores relacionados con la tarea actual
        // Señales del módulo Tasks
        public TaskSignals TaskSignals { get; init; } = default!;

        // Señales del módulo Habits
        public HabitSignals Habits { get; init; } = default!;
        // futuro

        // public StressSignals? Stress { get; init; }
    }
}
