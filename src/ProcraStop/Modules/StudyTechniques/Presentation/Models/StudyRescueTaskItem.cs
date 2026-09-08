using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Modules.StudyTechniques.Presentation.Models;

public sealed class StudyRescueTaskItem
{
    public StudyRescueTaskItem(TaskListItemDto task)
    {
        Id = task.Id;
        Title = task.Title;
        CourseId = task.CourseId;
        CourseName = string.IsNullOrWhiteSpace(task.CourseName) ? "Sin curso" : task.CourseName;
        EstimatedMinutes = task.EstimatedMinutes;
        Reason = BuildReason(task);
        Detail = BuildDetail(task);
    }

    public Guid Id { get; }
    public string Title { get; }
    public Guid? CourseId { get; }
    public string CourseName { get; }
    public int? EstimatedMinutes { get; }
    public string Reason { get; }
    public string Detail { get; }

    private static string BuildReason(TaskListItemDto task)
    {
        if (task.IsOverdue)
            return "Atrasada";

        if (task.Priority == TaskPriority.High)
            return "Alta prioridad";

        if (task.EstimatedMinutes > 60)
            return "Complejidad alta";

        return "Conviene preparar";
    }

    private static string BuildDetail(TaskListItemDto task)
    {
        var minutes = task.EstimatedMinutes.HasValue
            ? $"{task.EstimatedMinutes} min"
            : "sin tiempo estimado";
        var due = task.DueDate.HasValue
            ? task.DueDate.Value.ToString("dd MMM")
            : "sin fecha";

        return $"{minutes} · vence {due}";
    }
}
