using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Analytics;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Academic.Entities;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.UnitTests;

public sealed class CourseAppraisalTests
{
    [Fact]
    public void SetAppraisal_accepts_values_between_one_and_five()
    {
        var course = NewCourse();

        course.SetAppraisal(4, 2);

        Assert.Equal(4, course.InterestLevel);
        Assert.Equal(2, course.DifficultyLevel);
        Assert.NotNull(course.AppraisedAtUtc);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(6, null)]
    [InlineData(null, 0)]
    [InlineData(null, 6)]
    public void SetAppraisal_rejects_out_of_range_values(int? interest, int? difficulty)
    {
        var course = NewCourse();

        Assert.Throws<ArgumentOutOfRangeException>(() => course.SetAppraisal(interest, difficulty));
    }

    [Fact]
    public void SetAppraisal_can_clear_values()
    {
        var course = NewCourse();
        course.SetAppraisal(3, 3);

        course.SetAppraisal(null, null);

        Assert.Null(course.InterestLevel);
        Assert.Null(course.DifficultyLevel);
        Assert.Null(course.AppraisedAtUtc);
    }

    private static Course NewCourse() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Curso",
        CreatedAt = DateTime.UtcNow
    };
}

public sealed class TaskAnalyticsCourseMetricsTests
{
    [Fact]
    public async Task Groups_metrics_by_course_and_separates_readings()
    {
        var today = DateTime.Today;
        var courseA = Guid.NewGuid();
        var courseB = Guid.NewGuid();

        var tasks = new[]
        {
            // Curso A: bajo interés, alta dificultad, inicios tardíos.
            NewItem(courseA, "Cálculo", 1, 5, AcademicActivityType.Task,
                plannedStart: today.AddDays(-2), startedLocal: today, due: today.AddDays(2)),
            NewItem(courseA, "Cálculo", 1, 5, AcademicActivityType.Reading,
                plannedStart: today.AddDays(-1), startedLocal: today, due: today.AddDays(2)),

            // Curso B: alto interés, baja dificultad, inicios tempranos.
            NewItem(courseB, "Literatura", 5, 2, AcademicActivityType.Task,
                plannedStart: today.AddDays(1), startedLocal: today, due: today.AddDays(1).AddHours(10)),
            NewItem(courseB, "Literatura", 5, 2, AcademicActivityType.Reading,
                plannedStart: today.AddDays(1), startedLocal: today.AddDays(-1), due: today.AddDays(3)),

            // Sin curso: no debe mezclarse con los cursos.
            NewItem(null, null, null, null, AcademicActivityType.Task,
                plannedStart: today.AddDays(-1), startedLocal: null, due: today.AddDays(2))
        };

        var analytics = await new TaskAnalyticsService(new FakeReadService(tasks))
            .GetAsync(CancellationToken.None);

        Assert.Equal(3, analytics.CourseMetrics.Count);

        var cursoA = analytics.CourseMetrics.Single(m => m.CourseName == "Cálculo");
        Assert.Equal(courseA, cursoA.CourseId);
        Assert.Equal(1, cursoA.InterestLevel);
        Assert.Equal(5, cursoA.DifficultyLevel);
        Assert.Equal(2, cursoA.TotalTasks);
        Assert.Equal(1, cursoA.TotalReadings);
        Assert.Equal(1.5, cursoA.AvgStartDelayDays!.Value, precision: 3);
        Assert.Equal(0, cursoA.OnTimeStartRate, precision: 3);
        Assert.Equal(0, cursoA.LastMinuteStartRate, precision: 3);

        var cursoB = analytics.CourseMetrics.Single(m => m.CourseName == "Literatura");
        Assert.Equal(-1.5, cursoB.AvgStartDelayDays!.Value, precision: 3);
        Assert.Equal(1, cursoB.OnTimeStartRate, precision: 3);

        var sinCurso = analytics.CourseMetrics.Single(m => m.CourseName == "Sin curso");
        Assert.Null(sinCurso.CourseId);
        Assert.Equal(1, sinCurso.TotalTasks);
        Assert.Equal(1, sinCurso.OverdueStartNotStarted);

        Assert.Equal(2, analytics.ReadingMetrics.Count);
        var lecturaA = analytics.ReadingMetrics.Single(m => m.CourseName == "Cálculo");
        Assert.Equal(1, lecturaA.TotalTasks);
        Assert.Equal(1, lecturaA.AvgStartDelayDays!.Value, precision: 3);
        var lecturaB = analytics.ReadingMetrics.Single(m => m.CourseName == "Literatura");
        Assert.Equal(-2, lecturaB.AvgStartDelayDays!.Value, precision: 3);
    }

    private static TaskAnalyticsSourceItemDto NewItem(
        Guid? courseId,
        string? courseName,
        int? interest,
        int? difficulty,
        AcademicActivityType activityType,
        DateTime plannedStart,
        DateTime? startedLocal,
        DateTime due) => new()
        {
            CourseId = courseId,
            CourseName = courseName,
            CourseInterestLevel = interest,
            CourseDifficultyLevel = difficulty,
            ActivityType = activityType,
            CreatedAtUtc = LocalNoon(DateTime.Today.AddDays(-5)),
            PlannedStartDate = plannedStart,
            StartedAtUtc = startedLocal.HasValue ? LocalNoon(startedLocal.Value) : null,
            DueDate = due,
            Status = startedLocal.HasValue ? TaskItemStatus.InProgress : TaskItemStatus.Pending,
            Priority = TaskPriority.Medium
        };

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
