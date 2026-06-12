
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Commands.UpdateTask
{
    public sealed class UpdateTaskCommand
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }

        public DateTime? DueDate { get; init; }
        public DateTime? ReminderDate { get; init; }
        public ReminderOffset ReminderOffset { get; set; }
        public TaskPriority Priority { get; init; }
        public Guid? CourseId { get; init; }
        public int? EstimatedMinutes { get; init; }

        public List<SubtaskDto>? Subtasks { get; init; }
    }
}
