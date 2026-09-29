namespace ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;

public sealed class AcademicHelpRequestEntity
{
    public Guid Id { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? CourseId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public int Channel { get; set; }
    public string? ContactName { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RequestedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}
