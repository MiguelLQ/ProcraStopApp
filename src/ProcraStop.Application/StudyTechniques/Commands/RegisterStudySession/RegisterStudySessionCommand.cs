using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.Commands.RegisterStudySession;

public sealed class RegisterStudySessionCommand
{
    public Guid? TaskId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public StudyTechniqueType Technique { get; init; }
    public int PlannedMinutes { get; init; }
    public int CompletedMinutes { get; init; }
    public int FocusBefore { get; init; }
    public int EnergyBefore { get; init; }
    public int ConfidenceBefore { get; init; }
    public int ConfusionAfter { get; init; }
    public int ProgressPercent { get; init; }
    public int PomodoroCycles { get; init; }
    public AcademicHelpStatus HelpStatus { get; init; }
    public string? Notes { get; init; }
}
