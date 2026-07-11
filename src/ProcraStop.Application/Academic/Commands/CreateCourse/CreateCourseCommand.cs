
namespace ProcraStop.Application.Academic.Commands.CreateCourse
{
    public sealed class CreateCourseCommand
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Color { get; init; }
    }
}
