using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.Services;

public sealed class StudyRecommendationEngine
{
    private readonly StudyTechniqueCatalogService _catalog;

    public StudyRecommendationEngine(StudyTechniqueCatalogService catalog)
    {
        _catalog = catalog;
    }

    public StudyRecommendationDto Recommend(
        int focus,
        int energy,
        int confidence,
        int confusion,
        int availableMinutes,
        IReadOnlyCollection<StudyTechniqueSession> recentSessions)
    {
        var helpScore = CalculateHelpNeedScore(confidence, confusion, recentSessions);
        var technique = SelectTechnique(focus, energy, confidence, confusion, availableMinutes, recentSessions, helpScore);
        var info = _catalog.GetByType(technique);
        var shouldSeekHelp = helpScore >= 70;

        return new StudyRecommendationDto
        {
            Technique = technique,
            TechniqueName = info.Name,
            Message = BuildMessage(technique, shouldSeekHelp),
            Reason = BuildReason(focus, energy, confidence, confusion, helpScore),
            SuggestedAction = BuildSuggestedAction(technique, availableMinutes),
            RecommendedMinutes = Math.Min(Math.Max(10, availableMinutes), info.RecommendedMinutes),
            AcademicHelpNeedScore = helpScore,
            ShouldSeekHelp = shouldSeekHelp,
            HelpPrompt = shouldSeekHelp
                ? "Pide una pista, ejemplo similar o aclaracion puntual. Evita pedir la solucion completa."
                : "Aun puedes intentar una tecnica antes de pedir ayuda externa."
        };
    }

    public int CalculateHelpNeedScore(
        int confidence,
        int confusion,
        IReadOnlyCollection<StudyTechniqueSession> recentSessions)
    {
        var last = recentSessions.OrderByDescending(s => s.StartedAtUtc).Take(5).ToArray();
        var lowProgress = last.Count(s => s.ProgressPercent < 35) * 12;
        var highConfusion = last.Count(s => s.ConfusionAfter >= 4) * 10;
        var avoidedHelp = last.Count(s => s.HelpStatus == AcademicHelpStatus.Avoided) * 12;
        var baseScore = ((6 - Math.Clamp(confidence, 1, 5)) * 8) + (Math.Clamp(confusion, 1, 5) * 10);

        return Math.Clamp(baseScore + lowProgress + highConfusion + avoidedHelp, 0, 100);
    }

    private static StudyTechniqueType SelectTechnique(
        int focus,
        int energy,
        int confidence,
        int confusion,
        int availableMinutes,
        IReadOnlyCollection<StudyTechniqueSession> recentSessions,
        int helpScore)
    {
        if (availableMinutes <= 15 || focus <= 2 || energy <= 2)
            return StudyTechniqueType.Pomodoro;

        if (helpScore >= 75 || confusion >= 4 && confidence <= 2)
            return StudyTechniqueType.Feynman;

        if (recentSessions.Any(s => s.Technique == StudyTechniqueType.PracticeProblems && s.ProgressPercent < 45))
            return StudyTechniqueType.ErrorLog;

        if (confidence <= 2)
            return StudyTechniqueType.PracticeProblems;

        if (confusion >= 3)
            return StudyTechniqueType.ConceptMap;

        if (availableMinutes >= 45 && focus >= 4)
            return StudyTechniqueType.ExamSimulation;

        return StudyTechniqueType.ActiveRecall;
    }

    private static string BuildMessage(StudyTechniqueType technique, bool shouldSeekHelp)
    {
        if (shouldSeekHelp)
            return "Hay señales de bloqueo academico.";

        return technique switch
        {
            StudyTechniqueType.Pomodoro => "Empieza pequeño y protege el foco.",
            StudyTechniqueType.ActiveRecall => "Conviene practicar recuerdo activo.",
            StudyTechniqueType.Feynman => "Necesitas aclarar el concepto base.",
            StudyTechniqueType.PracticeProblems => "Lleva la teoria a ejercicios.",
            StudyTechniqueType.ExamSimulation => "Buen momento para medir preparacion.",
            _ => "Usa una tecnica guiada para avanzar."
        };
    }

    private static string BuildReason(int focus, int energy, int confidence, int confusion, int helpScore)
        => $"Foco {focus}/5, energia {energy}/5, confianza {confidence}/5, confusion {confusion}/5. Ayuda academica: {helpScore}/100.";

    private static string BuildSuggestedAction(StudyTechniqueType technique, int availableMinutes)
    {
        return technique switch
        {
            StudyTechniqueType.Pomodoro => availableMinutes <= 15
                ? "Haz un bloque de 10 minutos y registra solo el primer avance."
                : "Haz 25 minutos de foco y cierra con una nota de avance.",
            StudyTechniqueType.ActiveRecall => "Cierra apuntes, escribe lo que recuerdas y marca vacios.",
            StudyTechniqueType.Feynman => "Explica el tema en palabras simples y detecta donde te trabas.",
            StudyTechniqueType.PracticeProblems => "Resuelve un ejemplo guiado y luego uno sin mirar.",
            StudyTechniqueType.ErrorLog => "Revisa errores y escribe la causa real de cada fallo.",
            StudyTechniqueType.ExamSimulation => "Haz un bloque cronometrado y corrige al terminar.",
            _ => "Trabaja la tecnica y registra confusion, progreso y siguiente paso."
        };
    }
}
