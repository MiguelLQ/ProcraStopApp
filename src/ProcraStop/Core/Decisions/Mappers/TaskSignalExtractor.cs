
using ProcraStop.Core.Decisions.DTOs;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Modules.Tasks.Application.EngineTask.Signals;

namespace ProcraStop.Core.Decisions.Mappers
{
    public static class TaskSignalExtractor
    {
        public static IEnumerable<UnifiedSignal> Extract(TaskSignals tasks)
        {
            return new List<UnifiedSignal>
        {
            new()
            {
                Name = "Pending Tasks",
                Category = "Load",
                Score = Normalize(tasks.PendingCount, 10),
                Type = tasks.PendingCount == 0 ? DecisionType.Success : DecisionType.Info,
                Source = "Task"
            },

            new()
            {
                Name = "Overdue Pressure",
                Category = "Stress",
                Score = Normalize(tasks.OverdueCount, 5),
                Type = tasks.HasOverdueTasks ? DecisionType.Warning : DecisionType.Info,
                Source = "Task"
            },

            new()
            {
                Name = "Task Complexity Load",
                Category = "Performance",
                Score = Normalize(tasks.TotalEffort, 300),
                Type = tasks.HighTotalEffort ? DecisionType.Warning : DecisionType.Info,
                Source = "Task"
            }
        };
        }

        private static double Normalize(int value, int max)
        {
            return Math.Min(1.0, value / (double)max);
        }
    }
}
