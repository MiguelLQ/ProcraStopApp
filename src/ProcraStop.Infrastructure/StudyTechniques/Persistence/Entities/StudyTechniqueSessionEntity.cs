using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;

public sealed class StudyTechniqueSessionEntity
{
    public Guid Id { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public StudyTechniqueType Technique { get; set; }
    public int PlannedMinutes { get; set; }
    public int CompletedMinutes { get; set; }
    public int FocusBefore { get; set; }
    public int EnergyBefore { get; set; }
    public int ConfidenceBefore { get; set; }
    public int ConfusionAfter { get; set; }
    public int ProgressPercent { get; set; }
    public int PomodoroCycles { get; set; }
    public AcademicHelpStatus HelpStatus { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? Notes { get; set; }
}
