using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.DTOs
{
    public sealed class TaskListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime? DueDate { get; init; }
        public TaskPriority Priority { get; init; }
        public TaskItemStatus Status { get; init; }
        public bool IsArchived { get; init; }
        public int? EstimatedMinutes { get; init; }
        public bool IsOverdue { get; init; }
        // conexion con curso para ui listado
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseColor { get; set; }
    }
}
