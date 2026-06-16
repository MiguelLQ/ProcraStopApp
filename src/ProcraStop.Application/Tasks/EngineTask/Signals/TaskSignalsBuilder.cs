
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.EngineTask.Signals
{
    public class TaskSignalsBuilder
    {
        public TaskSignals Build(IReadOnlyList<TaskDashboardDto> tasks)
        {
            var pending = tasks
                .Where(t =>
                    t.Status != TaskItemStatus.Completed &&
                    !t.IsArchived)
                .ToList();

            var overdueCount = pending.Count(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value < DateTime.UtcNow);

            var quickTasks = pending.Count(t =>
                t.EstimatedMinutes.HasValue &&
                t.EstimatedMinutes.Value <= 30);

            var heavyTasks = pending.Count(t =>
                t.EstimatedMinutes.HasValue &&
                t.EstimatedMinutes.Value > 60);

            return new TaskSignals
            {
                PendingCount = pending.Count,

                OverdueCount = overdueCount,

                QuickTasksCount = quickTasks,

                HeavyTasksCount = heavyTasks,

                TotalEffort = pending.Sum(t => t.EstimatedMinutes ?? 30)
            };
        }
    }
}