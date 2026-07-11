

namespace ProcraStop.Application.Academic.Commands.UpdateCourse
{
    public sealed class UpdateCourseCommand
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? Color { get; init; }
    }
}
