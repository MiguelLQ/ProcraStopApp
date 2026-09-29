

using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.DTOs
{
    public sealed class TaskListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; } // editbycodex: permite busqueda local sin otra consulta.
        public DateTime? DueDate { get; init; }
        public TaskPriority Priority { get; init; }
        public TaskItemStatus Status { get; init; }
        public bool IsArchived { get; init; }
        public int? EstimatedMinutes { get; init; }
        public bool IsOverdue { get; init; }
        public bool HasSubtasks { get; init; } // editbycodex: proyeccion ligera para habilitar completado rapido.
        public AcademicActivityType ActivityType { get; init; }
        public DateTime? PlannedStartDate { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        // conexion con curso para ui listado
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseColor { get; set; }
    }
}
