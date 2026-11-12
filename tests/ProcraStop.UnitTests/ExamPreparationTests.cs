using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Analytics;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.UnitTests;

public sealed class ExamPreparationTests
{
    [Fact]
    public void ExtractTopics_uses_study_subtasks_and_ignores_generic_steps()
    {
        var topics = ExamTopicParser.ExtractTopics(new[]
        {
            "Estudiar: Álgebra",
            "Estudiar: Cálculo",
            "Recordar los temas sin apuntes y corregir errores",
            "Simulacro breve y revision de errores"
        });

        Assert.Equal(new[] { "Álgebra", "Cálculo" }, topics);
    }

    [Fact]
    public void ExtractTopics_returns_empty_when_only_generic_steps_exist()
    {
        var topics = ExamTopicParser.ExtractTopics(new[]
        {
            "Recordar los temas sin apuntes y corregir errores",
            "Simulacro breve y revision de errores"
        });

        Assert.Empty(topics);
    }

    [Fact]
    public async Task Coverage_links_sessions_to_the_exam_and_counts_pending_topics()
    {
        var examId = Guid.NewGuid();
        var exam = new TaskDetailDto
        {
            Id = examId,
            Title = "Examen de Cálculo",
            ActivityType = AcademicActivityType.Exam,
            DueDate = new DateTime(2026, 11, 12),
            Subtasks = new List<SubtaskDto>
            {
                new() { Title = "Estudiar: Álgebra", Id = Guid.NewGuid() },
                new() { Title = "Estudiar: Cálculo", Id = Guid.NewGuid() },
                new() { Title = "Estudiar: Repaso general", Id = Guid.NewGuid() },
                new() { Title = "Recordar los temas sin apuntes y corregir errores", Id = Guid.NewGuid() },
                new() { Title = "Simulacro breve y revision de errores", Id = Guid.NewGuid() }
            }
        };

        var sessions = new[]
        {
            Session(examId, "Álgebra", 25, new DateTime(2026, 11, 10, 9, 0, 0)),
            Session(examId, "Cálculo", 30, new DateTime(2026, 11, 11, 9, 0, 0))
        };

        var service = new ExamPreparationService(
            new FakeTaskReadService(exam),
            new FakeSessionRepository(sessions));

        var coverage = await service.GetCoverageAsync(examId, CancellationToken.None);

        Assert.NotNull(coverage);
        Assert.Equal(examId, coverage!.ExamTaskId);
        Assert.Equal(3, coverage.TotalTopics);
        Assert.Equal(2, coverage.CoveredTopics);
        Assert.Equal(2d / 3d, coverage.CoverageRate, precision: 3);
        Assert.Equal(new[] { "Repaso general" }, coverage.PendingTopics);
        Assert.Equal("Repaso general", coverage.SuggestedTopic);
        Assert.Equal(2, coverage.SessionsCount);
        Assert.Equal(55, coverage.TotalMinutes);
        Assert.Equal(1.5, coverage.AverageLeadTimeDays!.Value, precision: 3);
        Assert.Equal(LocalToUtc(new DateTime(2026, 11, 11, 9, 0, 0)), coverage.LastSessionAtUtc);
    }

    [Fact]
    public async Task Coverage_returns_null_for_a_task_that_is_not_an_exam()
    {
        var task = new TaskDetailDto
        {
            Id = Guid.NewGuid(),
            Title = "Tarea normal",
            ActivityType = AcademicActivityType.Task
        };

        var service = new ExamPreparationService(
            new FakeTaskReadService(task),
            new FakeSessionRepository(Array.Empty<StudyTechniqueSession>()));

        var coverage = await service.GetCoverageAsync(task.Id, CancellationToken.None);

        Assert.Null(coverage);
    }

    private static StudyTechniqueSession Session(
        Guid examId,
        string topic,
        int minutes,
        DateTime localStart) =>
        StudyTechniqueSession.Rehydrate(
            Guid.NewGuid(),
            examId,
            "Cálculo",
            topic,
            StudyTechniqueType.Pomodoro,
            plannedMinutes: minutes,
            completedMinutes: minutes,
            focusBefore: 3,
            energyBefore: 3,
            confidenceBefore: 3,
            confusionAfter: 2,
            progressPercent: 60,
            pomodoroCycles: 1,
            helpStatus: AcademicHelpStatus.NotNeeded,
            startedAtUtc: LocalToUtc(localStart),
            completedAtUtc: LocalToUtc(localStart.AddMinutes(minutes)),
            notes: null);

    private static DateTime LocalToUtc(DateTime local) =>
        DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();

    private sealed class FakeTaskReadService(TaskDetailDto task) : ITaskReadService
    {
        public Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct)
            => Task.FromResult<TaskDetailDto?>(id == task.Id ? task : null);

        public Task<IReadOnlyList<TaskListItemDto>> GetListAsync(bool includeArchived, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<TaskListItemDto>>(Array.Empty<TaskListItemDto>());

        public Task<IReadOnlyList<TaskListItemDto>> SearchAsync(
            string searchText, bool includeArchived, DateTime? dueDate, bool withoutDueDate,
            int maxResults, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<TaskDaySummaryDto>> GetDaySummariesAsync(
            DateTime startDate, DateTime endDate, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(CancellationToken ct)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<TaskAnalyticsSourceItemDto>> GetAnalyticsDataAsync(CancellationToken ct)
            => throw new NotSupportedException();
    }

    private sealed class FakeSessionRepository(
        IReadOnlyList<StudyTechniqueSession> sessions) : IStudyTechniqueSessionRepository
    {
        public Task AddAsync(StudyTechniqueSession session, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<StudyTechniqueSession>> GetRecentAsync(int days, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<StudyTechniqueSession>>(sessions);

        public Task<IReadOnlyList<StudyTechniqueSession>> GetByTaskIdAsync(Guid taskId, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<StudyTechniqueSession>>(
                sessions.Where(session => session.TaskId == taskId).ToArray());
    }
}
