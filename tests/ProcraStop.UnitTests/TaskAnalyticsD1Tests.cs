using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Analytics;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.UnitTests;

public sealed class TaskAnalyticsD1Tests
{
    [Fact]
    public async Task Computes_first_progress_delay_on_time_and_last_minute_metrics()
    {
        var today = DateTime.Today;
        var tasks = new[]
        {
            // Empezo 1 dia tarde; no fue al limite (faltaban mas de 24 h para vencer).
            NewItem(createdDaysAgo: 5, plannedStart: today.AddDays(-2), startedLocal: today.AddDays(-1),
                due: today.AddDays(1).AddHours(10)),

            // Empezo 1 dia antes; primer avance dentro de las ultimas 24 h antes de vencer.
            NewItem(createdDaysAgo: 5, plannedStart: today.AddDays(1), startedLocal: today,
                due: today.AddDays(1).AddHours(10)),

            // Debia empezar ayer y sigue sin iniciar.
            NewItem(createdDaysAgo: 2, plannedStart: today.AddDays(-1), startedLocal: null,
                due: today.AddDays(3)),

            // Sin inicio previsto: no entra en las metricas D1.
            NewItem(createdDaysAgo: 1, plannedStart: null, startedLocal: null, due: today.AddDays(2))
        };

        var analytics = await new TaskAnalyticsService(new FakeReadService(tasks))
            .GetAsync(CancellationToken.None);

        Assert.Equal(2, analytics.MeasuredStarts);
        Assert.Equal(2, analytics.MeasuredDueStarts);
        Assert.Equal(108, analytics.AvgHoursToFirstProgress!.Value, precision: 3);
        Assert.Equal(0, analytics.AvgStartDelayDays!.Value, precision: 3);
        Assert.Equal(0.5, analytics.OnTimeStartRate, precision: 3);
        Assert.Equal(0.5, analytics.LastMinuteStartRate, precision: 3);
        Assert.Equal(1, analytics.OverdueStartNotStarted);
    }

    [Fact]
    public async Task Metrics_are_empty_when_there_are_no_started_tasks()
    {
        var item = NewItem(createdDaysAgo: 1, plannedStart: DateTime.Today, startedLocal: null,
            due: DateTime.Today.AddDays(2));

        var analytics = await new TaskAnalyticsService(new FakeReadService([item]))
            .GetAsync(CancellationToken.None);

        Assert.Equal(0, analytics.MeasuredStarts);
        Assert.Null(analytics.AvgHoursToFirstProgress);
        Assert.Null(analytics.AvgStartDelayDays);
        Assert.Equal(0, analytics.OnTimeStartRate);
        Assert.Equal(0, analytics.LastMinuteStartRate);
    }

    private static TaskAnalyticsSourceItemDto NewItem(
        int createdDaysAgo,
        DateTime? plannedStart,
        DateTime? startedLocal,
        DateTime due) => new()
        {
            CreatedAtUtc = LocalNoon(DateTime.Today.AddDays(-createdDaysAgo)),
            PlannedStartDate = plannedStart,
            StartedAtUtc = startedLocal.HasValue ? LocalNoon(startedLocal.Value) : null,
            DueDate = due,
            Status = startedLocal.HasValue ? TaskItemStatus.InProgress : TaskItemStatus.Pending,
            Priority = TaskPriority.Medium
        };

    // Convierte una fecha local al mediodia a UTC para que ToLocalTime devuelva el mismo dia.
    private static DateTime LocalNoon(DateTime date) =>
        DateTime.SpecifyKind(date.Date.AddHours(12), DateTimeKind.Local).ToUniversalTime();

    private sealed class FakeReadService(IReadOnlyList<TaskAnalyticsSourceItemDto> items) : ITaskReadService
    {
        public Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskListItemDto>> GetListAsync(bool includeArchived, CancellationToken ct)
            => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskListItemDto>> SearchAsync(string searchText, bool includeArchived,
            DateTime? dueDate, bool withoutDueDate, int maxResults, CancellationToken ct)
            => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskDaySummaryDto>> GetDaySummariesAsync(DateTime startDate, DateTime endDate,
            CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(CancellationToken ct)
            => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskAnalyticsSourceItemDto>> GetAnalyticsDataAsync(CancellationToken ct)
            => Task.FromResult(items);
    }
}
