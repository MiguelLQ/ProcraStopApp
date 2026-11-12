namespace ProcraStop.Application.StudyTechniques.DTOs;

// D2 EPA: cobertura de un examen concreto a partir de las sesiones vinculadas.
public sealed class ExamCoverageDto
{
    public Guid ExamTaskId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public DateTime? DueDate { get; init; }
    public int? DaysUntilExam { get; init; }

    public int TotalTopics { get; init; }
    public int CoveredTopics { get; init; }
    public double CoverageRate { get; init; }
    public IReadOnlyList<string> PendingTopics { get; init; } = Array.Empty<string>();

    public int SessionsCount { get; init; }
    public int TotalMinutes { get; init; }
    public double? AverageLeadTimeDays { get; init; }
    public DateTime? LastSessionAtUtc { get; init; }

    public bool HasTopics => TotalTopics > 0;
    public bool HasSessions => SessionsCount > 0;
    public bool HasPendingTopics => PendingTopics.Count > 0;
    public string? SuggestedTopic => PendingTopics.FirstOrDefault();
}
