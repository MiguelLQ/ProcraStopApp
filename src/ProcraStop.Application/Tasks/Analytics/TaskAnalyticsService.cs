

using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Analytics
{
    public class TaskAnalyticsService
    {
        private readonly ITaskReadService _readService;

        public TaskAnalyticsService(ITaskReadService readService)
        {
            _readService = readService;
        }

        public async Task<TaskAnalyticsDto> GetAsync(CancellationToken ct)
        {
            var tasks = await _readService.GetListAsync(
                includeArchived: false,
                ct);

            return new TaskAnalyticsDto
            {
                Total = tasks.Count,

                Completed = tasks.Count(t =>
                    t.Status == TaskItemStatus.Completed),

                Pending = tasks.Count(t =>
                    t.Status == TaskItemStatus.Pending),

                Overdue = tasks.Count(t =>
                    t.DueDate < DateTime.UtcNow &&
                    t.Status != TaskItemStatus.Completed),

                HighPriority = tasks.Count(t =>
                    t.Priority == TaskPriority.High),

                MediumPriority = tasks.Count(t =>
                    t.Priority == TaskPriority.Medium),

                LowPriority = tasks.Count(t =>
                    t.Priority == TaskPriority.Low),

                ByCourse = tasks
                    .Where(t => t.CourseId != null)
                    .GroupBy(t => t.CourseName)
                    .ToDictionary(
                        g => g.Key ?? "Sin curso",
                        g => g.Count()),

                AvgEffort = tasks
                    .Where(t => t.EstimatedMinutes != null)
                    .Average(t => t.EstimatedMinutes ?? 0)
            };
        }
    }
}
