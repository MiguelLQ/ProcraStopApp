namespace ProcraStop.Infrastructure.Tasks.Persistence.Entities
{
    public sealed class SubtaskEntity
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }

        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? PlannedDate { get; set; }

        // navegación (opcional pero recomendado)
        public StudyTaskEntity Task { get; set; } = null!;
    }
}
