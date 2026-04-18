using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.Commands.UpdateTask
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
