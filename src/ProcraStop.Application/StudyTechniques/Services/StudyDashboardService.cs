using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.Services;

public sealed class StudyDashboardService
{
    private readonly IStudyTechniqueSessionRepository _repository;
    private readonly StudyRecommendationEngine _engine;
    private readonly StudyTechniqueCatalogService _catalog;

    public StudyDashboardService(
        IStudyTechniqueSessionRepository repository,
        StudyRecommendationEngine engine,
        StudyTechniqueCatalogService catalog)
    {
        _repository = repository;
        _engine = engine;
        _catalog = catalog;
    }

    public async Task<StudyDashboardDto> GetAsync(CancellationToken ct)
    {
        var sessions = await _repository.GetRecentAsync(7, ct);
        var helpScore = _engine.CalculateHelpNeedScore(3, 3, sessions);
        var mostUsed = sessions
            .GroupBy(s => s.Technique)
            .OrderByDescending(g => g.Count())
            .Select(g => _catalog.GetByType(g.Key).Name)
            .FirstOrDefault() ?? "Sin datos";
        var recommended = sessions.Any(s => s.ConfusionAfter >= 4)
            ? StudyTechniqueType.Feynman
            : StudyTechniqueType.Pomodoro;

        return new StudyDashboardDto
        {
            SessionsThisWeek = sessions.Count,
            MinutesThisWeek = sessions.Sum(s => s.CompletedMinutes),
            PomodorosThisWeek = sessions.Sum(s => s.PomodoroCycles),
            AverageFocus = sessions.Count == 0 ? 0 : (int)Math.Round(sessions.Average(s => s.FocusBefore)),
            AverageProgress = sessions.Count == 0 ? 0 : (int)Math.Round(sessions.Average(s => s.ProgressPercent)),
            AcademicHelpNeedScore = helpScore,
            MostUsedTechnique = mostUsed,
            HelpIndicatorText = helpScore >= 70 ? "Pedir ayuda" : helpScore >= 45 ? "Observar bloqueo" : "Controlado",
            RecommendedTechnique = recommended
        };
    }
}
