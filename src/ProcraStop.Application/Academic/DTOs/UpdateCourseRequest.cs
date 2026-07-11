

namespace ProcraStop.Application.Academic.DTOs
{
    public sealed class UpdateCourseRequest
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? Color { get; init; }
    }
}
