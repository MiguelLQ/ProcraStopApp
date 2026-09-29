using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Modules.Tasks.Presentation.Models;

public sealed record AcademicActivityOption(AcademicActivityType Value, string Name)
{
    public static IReadOnlyList<AcademicActivityOption> All { get; } =
    [
        new(AcademicActivityType.Task, "Tarea"),
        new(AcademicActivityType.Reading, "Lectura"),
        new(AcademicActivityType.Exam, "Examen"),
        new(AcademicActivityType.Deliverable, "Entregable")
    ];
}
