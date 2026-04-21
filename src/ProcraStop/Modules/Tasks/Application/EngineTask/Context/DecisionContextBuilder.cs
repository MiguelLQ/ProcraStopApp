using ProcraStop.Core.Decisions.Context;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;
using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.EngineTask.Context
{
    public class DecisionContextBuilder
    {
        private readonly TaskSignalsBuilder _taskBuilder;

        public DecisionContextBuilder(TaskSignalsBuilder taskBuilder)
        {
            _taskBuilder = taskBuilder;
        }

        public DecisionContext Build(IReadOnlyList<StudyTask> tasks)
        {
            return new DecisionContext
            {
                TaskSignals = _taskBuilder.Build(tasks),

                // futuro:
                // Habits = ...
                // Stress = ...
            };
        }
    }
}
