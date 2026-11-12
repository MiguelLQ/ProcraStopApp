namespace ProcraStop.Infrastructure.StudyTechniques.Persistence.Entities;

public sealed class ExamSimulationEntity
{
    public Guid Id { get; set; }
    public Guid ExamTaskId { get; set; }
    public int Score { get; set; }
    public string? Errors { get; set; }
    public DateTime TakenAtUtc { get; set; }
}
