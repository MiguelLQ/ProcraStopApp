using ProcraStop.Application.Notifications;
using ProcraStop.Domain.Tasks.Entities;

namespace ProcraStop.Application.Tasks.Services;

public static class TaskReminderPlanBuilder
{
    public const string DueKind = "DueReminder";
    public const string StartKind = "StartReminder";

    public static IReadOnlyList<ReminderSchedule> Build(StudyTask task, TimeProvider timeProvider)
    {
        if (task.IsArchived || task.IsCompleted) return [];
        var now = timeProvider.GetUtcNow();
        var localNow = timeProvider.GetLocalNow().DateTime;
        var result = new List<ReminderSchedule>(2);

        if (task.ReminderDate is { } due && due > localNow)
            result.Add(Create(task, DueKind, due, task.DueDate, now));

        var start = TaskStartReminderPolicy.ResolveNotificationTime(task, localNow);
        if (start is { } plannedStart)
            result.Add(Create(task, StartKind, plannedStart, null, now));

        return result;
    }

    private static ReminderSchedule Create(StudyTask task, string kind, DateTime scheduled,
        DateTime? due, DateTimeOffset now) => new(task.Id, kind, task.Title, task.Description,
        ToUtc(scheduled), due.HasValue ? ToUtc(due.Value) : null, (int)task.Priority,
        ReminderScheduleStatus.Pending, now);

    private static DateTimeOffset ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => new DateTimeOffset(value),
        DateTimeKind.Local => new DateTimeOffset(value).ToUniversalTime(),
        _ => new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(value), TimeSpan.Zero)
    };
}
