namespace ProcraStop.Infrastructure.Tasks.Persistence.Entities;

public sealed class ReminderScheduleEntity
{
    public Guid TaskId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset ScheduledForUtc { get; set; }
    public DateTimeOffset? DueDateUtc { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset? NextAttemptUtc { get; set; }
    public string? LastError { get; set; }
}
