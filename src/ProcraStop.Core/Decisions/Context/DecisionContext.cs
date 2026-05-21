using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Habits.Application.DTOs;
using ProcraStop.Modules.Stress.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Context
{
    public class DecisionContext
    {
        // Indicadores relacionados con la tarea actual
        // Señales del módulo Tasks
        public TaskSignals Tasks { get; init; } = default!;

        // Señales del módulo Habits
        public HabitSignals Habits { get; init; } = default!;
        // futuro

        public StressSignals Stress { get; init; } = default!;

        //
        // NUEVO (clave)
        public List<UnifiedSignal> Signals { get; init; } = new();
    }
}
