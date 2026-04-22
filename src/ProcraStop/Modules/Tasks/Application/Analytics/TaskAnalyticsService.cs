
using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.Analytics
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
            var tasks = await _readService.GetAllAsync(ct);

            return new TaskAnalyticsDto
            {
                Total = tasks.Count,
                Completed = tasks.Count(t => t.Status == (int)TaskItemStatus.Completed),
                Pending = tasks.Count(t => t.Status == (int)TaskItemStatus.Pending),
                Overdue = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != (int)TaskItemStatus.Completed),

                HighPriority = tasks.Count(t => t.Priority == (int)TaskPriority.High),
                MediumPriority = tasks.Count(t => t.Priority == (int)TaskPriority.Medium),
                LowPriority = tasks.Count(t => t.Priority == (int)TaskPriority.Low),

                ByCourse = tasks
                    .Where(t => t.CourseId != null)
                    .GroupBy(t => t.CourseName)
                    .ToDictionary(g => g.Key ?? "Sin curso", g => g.Count()),

                AvgEffort = tasks.Where(t => t.EstimatedMinutes != null)
                    .Average(t => t.EstimatedMinutes ?? 0)
            };
        }
    }
}
