

namespace ProcraStop.Application.Tasks.DTOs
{
    public sealed class SubtaskDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public bool IsCompleted { get; init; }
    }
}
