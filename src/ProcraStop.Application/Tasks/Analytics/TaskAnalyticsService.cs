

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

            // D1 EPA: "postergacion al ultimo momento".
            // Primer avance = primera vez que la tarea pasa a StartedAtUtc.
            var started = tasks.Where(t => t.StartedAtUtc.HasValue).ToArray();
            var firstProgressHours = started
                .Select(t => (t.StartedAtUtc!.Value - t.CreatedAtUtc).TotalHours)
                .Where(hours => hours >= 0)
                .ToArray();

            var withPlanAndStart = started
                .Where(t => t.PlannedStartDate.HasValue)
                .ToArray();

            // Diferencia en dias entre el inicio real y el inicio previsto.
            // Positivo = empezo tarde; negativo = empezo antes.
            var startDelays = withPlanAndStart
                .Select(t => (t.StartedAtUtc!.Value.ToLocalTime().Date - t.PlannedStartDate!.Value.Date).TotalDays)
                .ToArray();

            var startedWithDue = started
                .Where(t => t.DueDate.HasValue)
                .ToArray();

            var onTimeStarts = withPlanAndStart.Count(t =>
                t.StartedAtUtc!.Value.ToLocalTime().Date <= t.PlannedStartDate!.Value.Date);

            // Inicio al limite: el primer avance ocurre en las ultimas 24 horas antes del vencimiento
            // (o directamente despues de vencer).
            var lastMinuteStarts = startedWithDue.Count(t =>
                t.StartedAtUtc!.Value.ToLocalTime() >= t.DueDate!.Value.AddHours(-24));

            var overdueStartNotStarted = tasks.Count(t =>
                t.Status != TaskItemStatus.Completed &&
                !t.CompletedAtUtc.HasValue &&
                t.StartedAtUtc is null &&
                t.PlannedStartDate.HasValue &&
                t.PlannedStartDate.Value.Date < today);

            var readings = tasks
                .Where(t => t.ActivityType == AcademicActivityType.Reading)
                .ToArray();

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

                AvgHoursToFirstProgress = firstProgressHours.Length == 0
                    ? null
                    : firstProgressHours.Average(),
                AvgStartDelayDays = startDelays.Length == 0
                    ? null
                    : startDelays.Average(),
                OnTimeStartRate = withPlanAndStart.Length == 0
                    ? 0
                    : (double)onTimeStarts / withPlanAndStart.Length,
                LastMinuteStartRate = startedWithDue.Length == 0
                    ? 0
                    : (double)lastMinuteStarts / startedWithDue.Length,
                OverdueStartNotStarted = overdueStartNotStarted,
                MeasuredStarts = withPlanAndStart.Length,
                MeasuredDueStarts = startedWithDue.Length,
                CourseMetrics = BuildCourseMetrics(tasks, today),
                ReadingMetrics = BuildCourseMetrics(readings, today),

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

        private static IReadOnlyList<CourseProcrastinationMetricDto> BuildCourseMetrics(
            IReadOnlyList<TaskAnalyticsSourceItemDto> tasks,
            DateTime today)
        {
            return tasks
                .GroupBy(t => t.CourseId)
                .Select(group =>
                {
                    var items = group.ToArray();
                    var first = items[0];
                    var started = items.Where(t => t.StartedAtUtc.HasValue).ToArray();
                    var withPlan = started.Where(t => t.PlannedStartDate.HasValue).ToArray();
                    var startedWithDue = started.Where(t => t.DueDate.HasValue).ToArray();
                    var completedWithDue = items
                        .Where(t => t.CompletedAtUtc.HasValue && t.DueDate.HasValue)
                        .ToArray();

                    var delays = withPlan
                        .Select(t => (t.StartedAtUtc!.Value.ToLocalTime().Date - t.PlannedStartDate!.Value.Date).TotalDays)
                        .ToArray();
                    var leadTimes = startedWithDue
                        .Select(t => (t.DueDate!.Value - t.StartedAtUtc!.Value.ToLocalTime()).TotalDays)
                        .ToArray();
                    var completionMargins = completedWithDue
                        .Select(t => (t.DueDate!.Value - t.CompletedAtUtc!.Value.ToLocalTime()).TotalDays)
                        .ToArray();

                    return new CourseProcrastinationMetricDto
                    {
                        CourseId = group.Key,
                        CourseName = string.IsNullOrWhiteSpace(first.CourseName) ? "Sin curso" : first.CourseName!,
                        InterestLevel = first.CourseInterestLevel,
                        DifficultyLevel = first.CourseDifficultyLevel,
                        TotalTasks = items.Length,
                        TotalReadings = items.Count(t => t.ActivityType == AcademicActivityType.Reading),
                        MeasuredStarts = withPlan.Length,
                        MeasuredDueStarts = startedWithDue.Length,
                        AvgStartDelayDays = delays.Length == 0 ? null : delays.Average(),
                        OnTimeStartRate = withPlan.Length == 0
                            ? 0
                            : (double)withPlan.Count(t =>
                                t.StartedAtUtc!.Value.ToLocalTime().Date <= t.PlannedStartDate!.Value.Date) / withPlan.Length,
                        LastMinuteStartRate = startedWithDue.Length == 0
                            ? 0
                            : (double)startedWithDue.Count(t =>
                                t.StartedAtUtc!.Value.ToLocalTime() >= t.DueDate!.Value.AddHours(-24)) / startedWithDue.Length,
                        OverdueStartNotStarted = items.Count(t =>
                            t.Status != TaskItemStatus.Completed &&
                            !t.CompletedAtUtc.HasValue &&
                            t.StartedAtUtc is null &&
                            t.PlannedStartDate.HasValue &&
                            t.PlannedStartDate.Value.Date < today),
                        AvgLeadTimeDays = leadTimes.Length == 0 ? null : leadTimes.Average(),
                        AvgCompletionMarginDays = completionMargins.Length == 0 ? null : completionMargins.Average()
                    };
                })
                .OrderByDescending(metric => metric.HasEnoughData)
                .ThenByDescending(metric => metric.AvgStartDelayDays ?? double.MinValue)
                .ThenBy(metric => metric.CourseName)
                .ToArray();
        }
    }
}
