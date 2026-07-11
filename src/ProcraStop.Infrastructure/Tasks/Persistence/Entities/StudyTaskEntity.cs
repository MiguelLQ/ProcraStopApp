using ProcraStop.Infrastructure.Academic.Persistence.Entities;

namespace ProcraStop.Infrastructure.Tasks.Persistence.Entities
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
        public CourseEntity? Course { get; set; }
        public int? EstimatedMinutes { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public ICollection<SubtaskEntity> Subtasks { get; set; } = new List<SubtaskEntity>();
    }
}
