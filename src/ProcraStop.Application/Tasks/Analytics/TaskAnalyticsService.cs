

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
            var tasks = await _readService.GetAnalyticsDataAsync(ct);
            var now = DateTime.Now;
            var today = now.Date;
            var weekStart = today.AddDays(-6);
            var completed = tasks.Count(t => t.Status == TaskItemStatus.Completed);
            var pending = tasks.Count(t => t.Status == TaskItemStatus.Pending);
            var inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
            var weeklyDays = Enumerable.Range(0, 7)
                .Select(offset => weekStart.AddDays(offset))
                .ToArray();
            var courseGroups = tasks
                .GroupBy(t => string.IsNullOrWhiteSpace(t.CourseName) ? "Sin curso" : t.CourseName!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToArray(); // editbycodex: agrupa cursos una sola vez para etiquetas y valores.

            return new TaskAnalyticsDto
            {
                Total = tasks.Count,

                Completed = completed,

                Pending = pending,
                InProgress = inProgress,

                Overdue = tasks.Count(t =>
                    t.DueDate < now &&
                    t.Status != TaskItemStatus.Completed),

                HighPriority = tasks.Count(t =>
                    t.Priority == TaskPriority.High),

                MediumPriority = tasks.Count(t =>
                    t.Priority == TaskPriority.Medium),

                LowPriority = tasks.Count(t =>
                    t.Priority == TaskPriority.Low),

                ByCourse = courseGroups.ToDictionary(
                    group => group.Name,
                    group => group.Count), // editbycodex: reutiliza el unico agrupado calculado.

                AvgEffort = tasks
                    .Where(t => t.EstimatedMinutes != null)
                    .Select(t => (double)(t.EstimatedMinutes ?? 0))
                    .DefaultIfEmpty(0)
                    .Average(),

                CompletionRate = tasks.Count == 0 ? 0 : (double)completed / tasks.Count,
                CompletedThisWeek = tasks.Count(t =>
                    t.CompletedAtUtc.HasValue &&
                    t.CompletedAtUtc.Value.ToLocalTime().Date >= weekStart),
                DueNextSevenDays = tasks.Count(t =>
                    t.Status != TaskItemStatus.Completed &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value >= today &&
                    t.DueDate.Value < today.AddDays(7)),

                // editbycodex: series ya agregadas; la UI solo dibuja y no conoce reglas de negocio.
                WeeklyLabels = weeklyDays.Select(d => d.ToString("ddd")).ToArray(),
                WeeklyCompleted = weeklyDays
                    .Select(day => (double)tasks.Count(t =>
                        t.CompletedAtUtc.HasValue &&
                        t.CompletedAtUtc.Value.ToLocalTime().Date == day))
                    .ToArray(),
                StatusLabels = new[] { "Completadas", "Pendientes", "En curso" },
                StatusValues = new double[] { completed, pending, inProgress },
                PriorityLabels = new[] { "Alta", "Media", "Baja" },
                PriorityValues = new double[]
                {
                    tasks.Count(t => t.Priority == TaskPriority.High),
                    tasks.Count(t => t.Priority == TaskPriority.Medium),
                    tasks.Count(t => t.Priority == TaskPriority.Low)
                },
                CourseLabels = courseGroups.Select(g => g.Name).ToArray(),
                CourseValues = courseGroups.Select(g => (double)g.Count).ToArray()
            };
        }
    }
}
