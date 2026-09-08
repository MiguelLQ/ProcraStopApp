using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.DTOs;

public sealed class StudyRecommendationDto
{
    public StudyTechniqueType Technique { get; init; }
    public string TechniqueName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string SuggestedAction { get; init; } = string.Empty;
    public int RecommendedMinutes { get; init; }
    public int AcademicHelpNeedScore { get; init; }
    public bool ShouldSeekHelp { get; init; }
    public string HelpPrompt { get; init; } = string.Empty;
}
