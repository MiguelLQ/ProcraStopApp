

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

        // D1 EPA: valoracion opcional y declarada por el estudiante.
        // Se usa como variable de contexto, no como sustituto de la conducta.
        public int? InterestLevel { get; set; }
        public int? DifficultyLevel { get; set; }
        public DateTime? AppraisedAtUtc { get; set; }

        public void SetAppraisal(int? interestLevel, int? difficultyLevel)
        {
            ValidateAppraisal(interestLevel);
            ValidateAppraisal(difficultyLevel);

            InterestLevel = interestLevel;
            DifficultyLevel = difficultyLevel;
            AppraisedAtUtc = interestLevel.HasValue || difficultyLevel.HasValue
                ? DateTime.UtcNow
                : null;
        }

        private static void ValidateAppraisal(int? value)
        {
            if (value is < 1 or > 5)
                throw new ArgumentOutOfRangeException(nameof(value), "La valoración debe estar entre 1 y 5.");
        }

        // Navegación (solo dominio)
        private readonly List<StudyTask> _tasks = new();
        public IReadOnlyCollection<StudyTask> Tasks => _tasks;
    }
}
