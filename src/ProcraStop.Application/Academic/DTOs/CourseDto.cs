

namespace ProcraStop.Application.Academic.DTOs
{
    public sealed class CourseDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; } 

        public string? Color { get; init; }

        // D1 EPA: valoracion declarada opcional (1..5).
        public int? InterestLevel { get; init; }
        public int? DifficultyLevel { get; init; }
        public bool HasInterest => InterestLevel.HasValue;
        public bool HasDifficulty => DifficultyLevel.HasValue;
        public bool HasAppraisal => HasInterest || HasDifficulty;
    }
}
