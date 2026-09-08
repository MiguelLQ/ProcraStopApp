using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.DTOs;

public sealed class StudyTechniqueDto
{
    public StudyTechniqueType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string BestFor { get; init; } = string.Empty;
    public int RecommendedMinutes { get; init; }
}
