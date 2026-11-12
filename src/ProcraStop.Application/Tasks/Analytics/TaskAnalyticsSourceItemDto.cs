using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Analytics
{
    // editbycodex: proyeccion minima de persistencia usada solo para analitica.
    public sealed class TaskAnalyticsSourceItemDto
    {
        public DateTime? DueDate { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? PlannedStartDate { get; init; }
        public DateTime? StartedAtUtc { get; init; }
        public TaskPriority Priority { get; init; }
        public TaskItemStatus Status { get; init; }
        public AcademicActivityType ActivityType { get; init; }
        public int? EstimatedMinutes { get; init; }
        public Guid? CourseId { get; init; }
        public string? CourseName { get; init; }
        public int? CourseInterestLevel { get; init; }
        public int? CourseDifficultyLevel { get; init; }
    }
}
