using ProcraStop.Domain.StudyTechniques.Enums;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Models;

public sealed record HelpChannelOption(HelpChannel Value, string Name)
{
    public static IReadOnlyList<HelpChannelOption> All { get; } =
    [
        new(HelpChannel.CourseMaterial, "Material del curso"),
        new(HelpChannel.Classmate, "Compañero"),
        new(HelpChannel.Teacher, "Docente"),
        new(HelpChannel.AcademicSupport, "Tutoría o apoyo académico")
    ];
}
