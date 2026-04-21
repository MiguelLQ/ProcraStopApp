using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Context
{
    public class DecisionContext
    {
        // Indicadores relacionados con la tarea actual
        public object? TaskSignals { get; init; }
        public object? HabitSignals { get; init; }
        // futuro

        // public StressSignals? Stress { get; init; }
    }
}
