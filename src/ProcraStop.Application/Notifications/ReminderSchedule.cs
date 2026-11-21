namespace ProcraStop.Application.Notifications;

public enum ReminderScheduleStatus
{
    Active = 0,
    Delivered = 1,
    Cancelled = 2,
    Failed = 3,
    Pending = 4
}

public sealed record ReminderSchedule(
    Guid TaskId,
    string Kind,
    string Title,
    string? Description,
    DateTimeOffset ScheduledForUtc,
    DateTimeOffset? DueDateUtc,
    int Priority,
    ReminderScheduleStatus Status,
    DateTimeOffset UpdatedAtUtc)
{
    public int Attempts { get; init; }
    public DateTimeOffset? NextAttemptUtc { get; init; }
    public string? LastError { get; init; }
}

public interface IReminderScheduleRepository
{
    Task UpsertAsync(ReminderSchedule schedule, CancellationToken ct = default);
    Task<ReminderSchedule?> GetAsync(Guid taskId, string kind, CancellationToken ct = default);
    Task<IReadOnlyList<ReminderSchedule>> GetActiveAsync(CancellationToken ct = default);
    Task SetStatusAsync(Guid taskId, string kind, ReminderScheduleStatus status, CancellationToken ct = default);
    Task MarkAttemptAsync(Guid taskId, string kind, ReminderScheduleStatus status,
        DateTimeOffset? nextAttemptUtc, string? error, CancellationToken ct = default);
}
