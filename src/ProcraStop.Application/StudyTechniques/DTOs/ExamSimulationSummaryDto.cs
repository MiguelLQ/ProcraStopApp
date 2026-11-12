namespace ProcraStop.Application.StudyTechniques.DTOs;

// D2 EPA: resumen de simulacros de un examen concreto.
public sealed class ExamSimulationSummaryDto
{
    public Guid ExamTaskId { get; init; }
    public int Attempts { get; init; }
    public int? FirstScore { get; init; }
    public int? LastScore { get; init; }
    public int? BestScore { get; init; }
    public DateTime? LastTakenAtUtc { get; init; }
    public string? LastErrors { get; init; }

    public bool HasData => Attempts > 0;
    public int? Improvement => LastScore.HasValue && FirstScore.HasValue
        ? LastScore.Value - FirstScore.Value
        : null;
}
