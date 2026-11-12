

namespace ProcraStop.Application.Academic.DTOs
{
    public sealed class CreateCourseRequest
    {
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? Color { get; init; }
        public int? InterestLevel { get; init; }
        public int? DifficultyLevel { get; init; }
    }
}
