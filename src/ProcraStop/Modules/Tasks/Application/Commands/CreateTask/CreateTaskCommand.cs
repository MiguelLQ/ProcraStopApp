using ProcraStop.Modules.Tasks.Domain.Enums;


namespace ProcraStop.Modules.Tasks.Application.Commands.CreateTask
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
    }
}
