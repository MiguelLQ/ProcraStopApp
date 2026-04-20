
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DecisionEngine.Signals
{
    public class TaskSignalsBuilder
    {
        public TaskSignals Build(IReadOnlyList<StudyTask> tasks)
        {
            var pending = tasks
                .Where(t => !t.IsCompleted && !t.IsArchived)
                .ToList();

            return new TaskSignals
            {
                PendingCount = pending.Count,
                OverdueCount = pending.Count(t => t.IsOverdue),
                QuickTasksCount = pending.Count(t => t.GetEffort() == TaskEffort.Quick),
                HeavyTasksCount = pending.Count(t => t.GetEffort() == TaskEffort.Heavy),
                TotalEffort = pending.Sum(t => t.EstimatedMinutes ?? 30)
            };
        }
    }
}

