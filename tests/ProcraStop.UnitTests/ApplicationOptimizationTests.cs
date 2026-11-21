using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Application.Habits.Interfaces;
using ProcraStop.Application.Habits.Queries.GetHabitSignals;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Analytics;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Queries.GetTasks;
using ProcraStop.Core.Decisions.Signals;
using ProcraStop.Domain.Habits.Entities;

namespace ProcraStop.UnitTests;

public sealed class ApplicationOptimizationTests
{
    [Fact]
    public async Task GetTasks_UsesBoundedPageAtExpectedOffset()
    {
        var readService = new CapturingTaskReadService();
        var handler = new GetTasksQueryHandler(readService);

        await handler.HandleAsync(new GetTasksQuery
        {
            IncludeArchived = true,
            PageNumber = 3,
            PageSize = 250
        });

        Assert.True(readService.IncludeArchived);
        Assert.Equal(200, readService.Skip);
        Assert.Equal(100, readService.Take);
    }

    [Fact]
    public async Task HabitSignals_ReadsOnlyActiveHabitsAndSevenDayWindow()
    {
        var habits = new CapturingHabitRepository();
        var logs = new CapturingHabitLogRepository();
        var metrics = new CapturingHabitMetricsService();
        var handler = new GetHabitSignalsHandler(habits, logs, metrics);

        await handler.Handle(new GetHabitSignalsQuery());

        Assert.Equal(logs.EndDate.AddDays(-6), logs.StartDate);
        Assert.Equal(DateTime.UtcNow.Date, logs.EndDate);
        Assert.Equal(logs.EndDate, metrics.Today);
        Assert.True(habits.ActiveRequested);
    }

    private sealed class CapturingTaskReadService : ITaskReadService
    {
        public bool IncludeArchived { get; private set; }
        public int Skip { get; private set; }
        public int Take { get; private set; }

        public Task<IReadOnlyList<TaskListItemDto>> GetPageAsync(
            bool includeArchived,
            int skip,
            int take,
            CancellationToken ct)
        {
            IncludeArchived = includeArchived;
            Skip = skip;
            Take = take;
            return Task.FromResult<IReadOnlyList<TaskListItemDto>>([]);
        }

        public Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskListItemDto>> GetListAsync(bool includeArchived, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskListItemDto>> SearchAsync(string searchText, bool includeArchived, DateTime? dueDate, bool withoutDueDate, int maxResults, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskDaySummaryDto>> GetDaySummariesAsync(DateTime startDate, DateTime endDate, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<TaskAnalyticsSourceItemDto>> GetAnalyticsDataAsync(CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class CapturingHabitRepository : IHabitRepository
    {
        public bool ActiveRequested { get; private set; }

        public Task<IReadOnlyList<Habit>> GetActiveAsync(CancellationToken ct)
        {
            ActiveRequested = true;
            return Task.FromResult<IReadOnlyList<Habit>>([]);
        }

        public Task<List<Habit>> GetAllAsync() => throw new InvalidOperationException("No debe cargar todos los hábitos.");
        public Task AddAsync(Habit habit) => throw new NotSupportedException();
        public Task<Habit?> GetByIdAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task UpdateAsync(Habit habit, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class CapturingHabitLogRepository : IHabitLogRepository
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public Task<IReadOnlyList<HabitLog>> GetRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            StartDate = startDate;
            EndDate = endDate;
            return Task.FromResult<IReadOnlyList<HabitLog>>([]);
        }

        public Task<List<HabitLog>> GetAllAsync() => throw new InvalidOperationException("No debe cargar todo el historial.");
        public Task AddAsync(HabitLog log) => throw new NotSupportedException();
        public Task<HabitLog?> GetByHabitAndDateAsync(Guid habitId, DateTime date) => throw new NotSupportedException();
        public Task UpsertAsync(HabitLog log) => throw new NotSupportedException();
        public Task<IReadOnlyList<HabitLog>> GetRangeForHabitAsync(Guid habitId, DateTime startDate, DateTime endDate, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class CapturingHabitMetricsService : IHabitMetricsService
    {
        public DateTime Today { get; private set; }

        public HabitSignals Calculate(
            IReadOnlyCollection<Habit> habits,
            IReadOnlyCollection<HabitLog> logs,
            DateTime today)
        {
            Today = today;
            return HabitSignals.Empty();
        }
    }
}
