namespace ProcraStop.Application.Notifications;

public enum ReminderEventStatus
{
    ScheduledExact,
    ScheduledInexact,
    Delivered,
    Cancelled,
    PermissionMissing,
    Failed,
    Reconciled
}

public sealed record ReminderHistoryItem(
    Guid Id,
    Guid TaskId,
    string Kind,
    ReminderEventStatus Status,
    string Title,
    DateTime? ScheduledForUtc,
    DateTime OccurredAtUtc,
    string? Details);

public sealed record ReminderCapabilityStatus(
    bool NotificationsGranted,
    bool ExactAlarmsGranted,
    bool DueChannelEnabled,
    bool StartChannelEnabled,
    bool LockScreenContentEnabled,
    bool CanDeliverPrecisely,
    string Message);
