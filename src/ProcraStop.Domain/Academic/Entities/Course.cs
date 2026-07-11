

using ProcraStop.Domain.Tasks.Entities;

namespace ProcraStop.Domain.Academic.Entities
{
    public sealed class Course
    {
        public Guid Id { get; init; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navegación (solo dominio)
        private readonly List<StudyTask> _tasks = new();
        public IReadOnlyCollection<StudyTask> Tasks => _tasks;
    }
}
