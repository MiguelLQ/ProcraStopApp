using ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals;
using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Context
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
                Tasks = _taskBuilder.Build(tasks),

                // futuro:
                // Habits = ...
                // Stress = ...
            };
        }
    }
}
