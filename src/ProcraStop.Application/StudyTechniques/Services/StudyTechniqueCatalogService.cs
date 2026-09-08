using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Application.StudyTechniques.Services;

public sealed class StudyTechniqueCatalogService
{
    private static readonly StudyTechniqueDto[] Techniques =
    {
        new() { Type = StudyTechniqueType.Pomodoro, Name = "Pomodoro", Purpose = "Entrar en foco con bloques cortos.", BestFor = "Baja energia, inicio dificil o distracciones.", RecommendedMinutes = 25 },
        new() { Type = StudyTechniqueType.ActiveRecall, Name = "Active Recall", Purpose = "Recordar sin mirar apuntes.", BestFor = "Memoria debil o examen cercano.", RecommendedMinutes = 30 },
        new() { Type = StudyTechniqueType.SpacedRepetition, Name = "Repaso espaciado", Purpose = "Retener a largo plazo.", BestFor = "Cursos con mucha teoria o formulas.", RecommendedMinutes = 20 },
        new() { Type = StudyTechniqueType.Feynman, Name = "Feynman", Purpose = "Explicar simple lo complejo.", BestFor = "Confusion conceptual.", RecommendedMinutes = 25 },
        new() { Type = StudyTechniqueType.CornellNotes, Name = "Notas Cornell", Purpose = "Ordenar clase y preguntas clave.", BestFor = "Lecturas o clases densas.", RecommendedMinutes = 35 },
        new() { Type = StudyTechniqueType.Interleaving, Name = "Interleaving", Purpose = "Mezclar tipos de problema.", BestFor = "Practica avanzada y examenes.", RecommendedMinutes = 40 },
        new() { Type = StudyTechniqueType.PracticeProblems, Name = "Ejercicios guiados", Purpose = "Convertir teoria en ejecucion.", BestFor = "Cuando entiendes pero fallas ejercicios.", RecommendedMinutes = 45 },
        new() { Type = StudyTechniqueType.ExamSimulation, Name = "Simulacro", Purpose = "Medir preparacion real.", BestFor = "Examen cercano.", RecommendedMinutes = 50 },
        new() { Type = StudyTechniqueType.ConceptMap, Name = "Mapa conceptual", Purpose = "Conectar ideas.", BestFor = "Temas amplios o desordenados.", RecommendedMinutes = 25 },
        new() { Type = StudyTechniqueType.ErrorLog, Name = "Diario de errores", Purpose = "Aprender de fallos repetidos.", BestFor = "Practica con errores frecuentes.", RecommendedMinutes = 20 }
    };

    public IReadOnlyList<StudyTechniqueDto> GetAll() => Techniques;

    public StudyTechniqueDto GetByType(StudyTechniqueType type)
        => Techniques.FirstOrDefault(t => t.Type == type) ?? Techniques[0];
}
