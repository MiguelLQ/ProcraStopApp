namespace ProcraStop.Infrastructure.Tasks.Persistence.Entities;

public sealed class ReminderHistoryEntity
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? ScheduledForUtc { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? Details { get; set; }
}
