using ProcraStop.Application.StudyTechniques.DTOs;
using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Models;

public sealed class StudyTechniqueOption
{
    public StudyTechniqueOption(StudyTechniqueDto dto)
    {
        Type = dto.Type;
        Name = dto.Name;
        Purpose = dto.Purpose;
        BestFor = dto.BestFor;
        RecommendedMinutes = dto.RecommendedMinutes;
    }

    public StudyTechniqueType Type { get; }
    public string Name { get; }
    public string Purpose { get; }
    public string BestFor { get; }
    public int RecommendedMinutes { get; }
}
