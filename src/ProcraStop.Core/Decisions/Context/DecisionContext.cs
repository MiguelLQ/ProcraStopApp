
using ProcraStop.Core.Decisions.Signals;
namespace ProcraStop.Core.Decisions.Context
{
    public class DecisionContext
    {
        public TaskSignals Tasks { get; }
        public HabitSignals Habits { get; }
        public StressSignals Stress { get; }
        public List<UnifiedSignal> Signals { get; }

        public DecisionContext(
            TaskSignals tasks,
            HabitSignals habits,
            StressSignals stress,
            List<UnifiedSignal> signals)
        {
            Tasks = tasks;
            Habits = habits;
            Stress = stress;
            Signals = signals;
        }
    }
}
