using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.StudyTechniques.Services;

// D2 EPA: une las sesiones de estudio con un examen concreto y calcula cobertura.
public sealed class ExamPreparationService
{
    private readonly ITaskReadService _taskReadService;
    private readonly IStudyTechniqueSessionRepository _sessionRepository;

    public ExamPreparationService(
        ITaskReadService taskReadService,
        IStudyTechniqueSessionRepository sessionRepository)
    {
        _taskReadService = taskReadService;
        _sessionRepository = sessionRepository;
    }

    public async Task<ExamCoverageDto?> GetCoverageAsync(
        Guid examTaskId,
        CancellationToken ct = default)
    {
        var exam = await _taskReadService.GetByIdAsync(examTaskId, ct);
        if (exam is null || exam.ActivityType != AcademicActivityType.Exam)
            return null;

        var sessions = await _sessionRepository.GetByTaskIdAsync(examTaskId, ct);
        return Build(exam, sessions, DateTime.UtcNow);
    }

    public static ExamCoverageDto Build(
        TaskDetailDto exam,
        IReadOnlyList<StudyTechniqueSession> sessions,
        DateTime nowUtc)
    {
        var topics = ExamTopicParser.ExtractTopics(exam.Subtasks.Select(s => s.Title));
        var sessionTopics = sessions
            .Select(session => ExamTopicParser.NormalizeTopic(session.Topic))
            .Where(topic => topic.Length > 0)
            .ToArray();

        var coveredTopics = topics
            .Where(topic => sessionTopics.Any(sessionTopic =>
                string.Equals(sessionTopic, topic, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var pendingTopics = topics
            .Where(topic => !coveredTopics.Contains(topic, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        var leadTimes = exam.DueDate.HasValue
            ? sessions
                .Select(session => (exam.DueDate.Value.Date - session.StartedAtUtc.ToLocalTime().Date).TotalDays)
                .Where(days => days >= 0)
                .ToArray()
            : Array.Empty<double>();

        var daysUntilExam = exam.DueDate.HasValue
            ? (int)Math.Ceiling(
                (exam.DueDate.Value.Date - nowUtc.ToLocalTime().Date).TotalDays)
            : (int?)null;

        return new ExamCoverageDto
        {
            ExamTaskId = exam.Id,
            ExamTitle = exam.Title,
            DueDate = exam.DueDate,
            DaysUntilExam = daysUntilExam,
            TotalTopics = topics.Count,
            CoveredTopics = coveredTopics.Length,
            CoverageRate = topics.Count == 0
                ? 0
                : (double)coveredTopics.Length / topics.Count,
            PendingTopics = pendingTopics,
            SessionsCount = sessions.Count,
            TotalMinutes = sessions.Sum(session => session.CompletedMinutes),
            AverageLeadTimeDays = leadTimes.Length == 0
                ? null
                : leadTimes.Average(),
            LastSessionAtUtc = sessions
                .Select(session => (DateTime?)session.StartedAtUtc)
                .Max()
        };
    }
}
