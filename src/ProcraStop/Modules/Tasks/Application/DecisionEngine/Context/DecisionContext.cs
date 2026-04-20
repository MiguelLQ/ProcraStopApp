
using ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Context
{
    public class DecisionContext
    {
        // Indicadores relacionados con la tarea actual
        public TaskSignals Tasks { get; init; } = default!;

        // futuro
        //public HabitSignals? Habits { get; init; }

        // public StressSignals? Stress { get; init; }
    }
}
