using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Infraestructure.Persistence
{
    public sealed class StudyTaskEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public bool IsArchived { get; set; }
        public Guid? CourseId { get; set; }
        public int? EstimatedMinutes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }
}
