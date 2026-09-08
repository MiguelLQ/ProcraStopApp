using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.DTOs;

public sealed class StudyDashboardDto
{
    public int SessionsThisWeek { get; init; }
    public int MinutesThisWeek { get; init; }
    public int PomodorosThisWeek { get; init; }
    public int AverageFocus { get; init; }
    public int AverageProgress { get; init; }
    public int AcademicHelpNeedScore { get; init; }
    public string MostUsedTechnique { get; init; } = "Sin datos";
    public string HelpIndicatorText { get; init; } = "Sin datos";
    public StudyTechniqueType RecommendedTechnique { get; init; } = StudyTechniqueType.Pomodoro;
}
