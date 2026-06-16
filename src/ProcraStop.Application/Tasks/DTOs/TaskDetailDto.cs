


using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.DTOs
{
    public sealed class TaskDetailDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }

        public DateTime? DueDate { get; init; }
        public DateTime? ReminderDate { get; init; }
        public ReminderOffset ReminderOffset { get; set; }
        public int Priority { get; init; }
        public int Status { get; init; }

        public int? EstimatedMinutes { get; init; }

        public Guid? CourseId { get; init; }
        public string? CourseName { get; init; }
        public string? CourseColor { get; init; }
        public List<SubtaskDto> Subtasks { get; init; } = new();

      
    }
}
