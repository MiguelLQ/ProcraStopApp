namespace ProcraStop.Infrastructure.Academic.Persistence.Entities
{
    public sealed class CourseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Color { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
