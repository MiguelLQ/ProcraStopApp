using ProcraStop.Modules.Tasks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Domain.Entities
{
    public sealed class StudyTask
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? DueDate { get; init; }
        public DateTime? ReminderDate { get; init; }
        public TaskPriority Priority { get; init; }
        public TaskItemStatus Status { get; init; }
        public bool IsArchived { get; init; }
        public Guid? CourseId { get; init; }
        public int? EstimatedMinutes { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }

        public bool IsOverdue =>
            DueDate.HasValue &&
            DueDate.Value < DateTime.Now &&
            Status != TaskItemStatus.Completed &&
            !IsArchived;
    }
}
