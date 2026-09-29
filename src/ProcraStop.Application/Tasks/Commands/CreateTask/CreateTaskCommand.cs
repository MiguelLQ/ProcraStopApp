
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Commands.CreateTask
{
    public sealed class CreateTaskCommand
    {
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? DueDate { get; init; }
        public DateTime? ReminderDate { get; init; }
        public TaskPriority Priority { get; init; } = TaskPriority.Medium;
        public Guid? CourseId { get; init; }
        public int? EstimatedMinutes { get; init; }
        public ReminderOffset ReminderOffset { get; set; }
        // NUEVO: Subtasks
        public List<string>? Subtasks { get; init; }
        public AcademicActivityType ActivityType { get; init; } = AcademicActivityType.Task;
        public DateTime? PlannedStartDate { get; init; }
        public string? FirstStep { get; init; }
        public DateTime? PlannedReviewDate { get; init; }
        public List<PlannedStepDto>? Milestones { get; init; }
    }
}
