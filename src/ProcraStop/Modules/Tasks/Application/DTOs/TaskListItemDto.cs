using ProcraStop.Modules.Tasks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.DTOs
{
    public sealed class TaskListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? DueDate { get; init; }
        public TaskPriority Priority { get; init; }
        public TaskItemStatus Status { get; init; }
        public bool IsArchived { get; init; }
        public int? EstimatedMinutes { get; init; }
        public bool IsOverdue { get; init; }
    }
}
