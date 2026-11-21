using Android.App;
using Android.Content;
using Android.OS;
using ProcraStop.Application.Notifications;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Enums;
using AndroidApp = Android.App.Application;

namespace ProcraStop.Maui11.Platforms.Android.Services;

public sealed class AndroidTaskReminderService : ITaskReminderService
{
    internal const string DueKind = "DueReminder";
    internal const string StartKind = "StartReminder";
    internal const string DueChannelId = "task_due_reminders_v3";
    internal const string StartChannelId = "task_start_reminders_v3";
    internal const string SummaryChannelId = "task_summary_v1";
    private const string AlarmAction = "com.companyname.procrastop.maui11.TASK_REMINDER";

    private readonly IReminderScheduleRepository _schedules;
    private readonly IReminderHistoryRepository _history;
    private readonly TimeProvider _timeProvider;
    private static Context Context => AndroidApp.Context;

    public AndroidTaskReminderService(
        IReminderScheduleRepository schedules,
        IReminderHistoryRepository history,
        TimeProvider? timeProvider = null)
    {
        _schedules = schedules;
        _history = history;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task ScheduleAsync(Guid taskId, string title, string? description, DateTime reminderDate,
        DateTime? dueDate, TaskPriority priority, CancellationToken cancellationToken = default) =>
        ScheduleAsync(taskId, DueKind, title, description, reminderDate, dueDate, priority, cancellationToken);

    public Task ScheduleStartReminderAsync(Guid taskId, string title, string? description,
        DateTime plannedStartDate, TaskPriority priority, CancellationToken cancellationToken = default) =>
        ScheduleAsync(taskId, StartKind, title, description, plannedStartDate, null, priority, cancellationToken);

    private async Task ScheduleAsync(Guid taskId, string kind, string title, string? description,
        DateTime scheduledFor, DateTime? dueDate, TaskPriority priority, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var utc = ToUtc(scheduledFor);
        var schedule = new ReminderSchedule(taskId, kind, title, description, utc,
            dueDate.HasValue ? ToUtc(dueDate.Value) : null, (int)priority,
            ReminderScheduleStatus.Pending, now);

        await _schedules.UpsertAsync(schedule, ct);

        if (utc <= now)
        {
            await _schedules.MarkAttemptAsync(taskId, kind, ReminderScheduleStatus.Failed,
                null, "La fecha ya pasó", ct);
            await _history.AddAsync(Event(schedule, ReminderEventStatus.Failed, "La fecha ya pasó"), ct);
            return;
        }

        try
        {
            CancelAlarm(taskId, kind);
            var exact = Arm(schedule);
            await _schedules.MarkAttemptAsync(taskId, kind, ReminderScheduleStatus.Active,
                null, null, ct);
            await _history.AddAsync(Event(schedule,
                exact ? ReminderEventStatus.ScheduledExact : ReminderEventStatus.ScheduledInexact,
                exact ? "AlarmManager exacto" : "Permiso exacto ausente"), ct);
        }
        catch (Exception ex)
        {
            await _schedules.MarkAttemptAsync(taskId, kind, ReminderScheduleStatus.Pending,
                now.AddMinutes(5), ex.Message, ct);
            await _history.AddAsync(Event(schedule, ReminderEventStatus.Failed,
                $"Pendiente de reintento: {ex.Message}"), ct);
        }
    }

    public Task CancelDueReminderAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        CancelAsync(taskId, DueKind, cancellationToken);

    public Task CancelStartReminderAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        CancelAsync(taskId, StartKind, cancellationToken);

    public async Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await CancelAsync(taskId, DueKind, cancellationToken);
        await CancelAsync(taskId, StartKind, cancellationToken);
    }

    private async Task CancelAsync(Guid taskId, string kind, CancellationToken ct)
    {
        var schedule = await _schedules.GetAsync(taskId, kind, ct);
        CancelAlarm(taskId, kind);
        ((NotificationManager?)Context.GetSystemService(Context.NotificationService))
            ?.Cancel(taskId.ToString("D"), NotificationId(kind));
        await _schedules.SetStatusAsync(taskId, kind, ReminderScheduleStatus.Cancelled, ct);
        if (schedule is not null)
            await _history.AddAsync(Event(schedule, ReminderEventStatus.Cancelled, null), ct);
    }

    public async Task ReconcileAsync(bool audit, CancellationToken ct = default)
    {
        foreach (var schedule in await _schedules.GetActiveAsync(ct))
        {
            var now = _timeProvider.GetUtcNow();
            if (schedule.ScheduledForUtc <= now)
            {
                await _schedules.SetStatusAsync(schedule.TaskId, schedule.Kind, ReminderScheduleStatus.Failed, ct);
                await _history.AddAsync(Event(schedule, ReminderEventStatus.Failed, "Venció sin entrega confirmada"), ct);
                continue;
            }

            if (schedule.NextAttemptUtc is { } retryAt && retryAt > now) continue;
            try
            {
                var exact = Arm(schedule);
                await _schedules.MarkAttemptAsync(schedule.TaskId, schedule.Kind,
                    ReminderScheduleStatus.Active, null, null, ct);
                if (audit)
                    await _history.AddAsync(Event(schedule, ReminderEventStatus.Reconciled,
                        exact ? "Reprogramado exacto" : "Reprogramado aproximado"), ct);
            }
            catch (Exception ex)
            {
                var delay = TimeSpan.FromMinutes(Math.Min(60, 5 * Math.Max(1, schedule.Attempts + 1)));
                await _schedules.MarkAttemptAsync(schedule.TaskId, schedule.Kind,
                    ReminderScheduleStatus.Pending, now.Add(delay), ex.Message, ct);
            }
        }
    }

    public async Task DeliverAsync(Guid taskId, string kind, long expectedUtcTicks, CancellationToken ct = default)
    {
        var schedule = await _schedules.GetAsync(taskId, kind, ct);
        if (schedule is null || schedule.Status != ReminderScheduleStatus.Active ||
            schedule.ScheduledForUtc.Ticks != expectedUtcTicks) return;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
            Context.CheckSelfPermission("android.permission.POST_NOTIFICATIONS") !=
            global::Android.Content.PM.Permission.Granted)
        {
            await _schedules.SetStatusAsync(taskId, kind, ReminderScheduleStatus.Failed, ct);
            await _history.AddAsync(Event(schedule, ReminderEventStatus.PermissionMissing, "POST_NOTIFICATIONS"), ct);
            return;
        }

        var manager = (NotificationManager?)Context.GetSystemService(Context.NotificationService);
        if (manager is null) return;
        var isStart = kind == StartKind;
        var channel = isStart ? StartChannelId : DueChannelId;
        var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O
            ? new Notification.Builder(Context, channel)
            : new Notification.Builder(Context);
        builder.SetSmallIcon(Resource.Drawable.ic_task_reminder)
            .SetContentTitle(isStart ? $"🚀 ¿Empezamos? {schedule.Title}" : $"⏰ {schedule.Title}")
            .SetContentText(BuildBody(schedule, isStart))
            .SetAutoCancel(true)
            .SetContentIntent(OpenTaskIntent(taskId))
            .SetCategory(Notification.CategoryReminder)
            .SetVisibility(NotificationVisibility.Public)
            .SetPriority((int)NotificationPriority.High);
        manager.Notify(taskId.ToString("D"), NotificationId(kind), builder.Build());

        await _schedules.SetStatusAsync(taskId, kind, ReminderScheduleStatus.Delivered, ct);
        var drift = (_timeProvider.GetUtcNow() - schedule.ScheduledForUtc).TotalSeconds;
        await _history.AddAsync(Event(schedule, ReminderEventStatus.Delivered,
            $"Desfase {drift:F0} segundos"), ct);
    }

    private static bool Arm(ReminderSchedule schedule)
    {
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService)
            ?? throw new InvalidOperationException("AlarmManager no disponible.");
        var pending = AlarmIntent(schedule, PendingIntentFlags.UpdateCurrent)!;
        var at = schedule.ScheduledForUtc.ToUnixTimeMilliseconds();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarm.CanScheduleExactAlarms())
        {
            alarm.SetAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
            return false;
        }
        if (schedule.Kind == DueKind && schedule.Priority == (int)TaskPriority.High)
            alarm.SetAlarmClock(new AlarmManager.AlarmClockInfo(at, OpenTaskIntent(schedule.TaskId)), pending);
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            alarm.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
        else
            alarm.SetExact(AlarmType.RtcWakeup, at, pending);
        return true;
    }

    private static PendingIntent? AlarmIntent(ReminderSchedule schedule, PendingIntentFlags flags)
    {
        var intent = new Intent(Context, typeof(TaskReminderReceiver));
        intent.SetAction(AlarmAction);
        intent.SetData(global::Android.Net.Uri.Parse($"procrastop://reminder/{schedule.Kind}/{schedule.TaskId:D}"));
        intent.PutExtra("taskId", schedule.TaskId.ToString("D"));
        intent.PutExtra("kind", schedule.Kind);
        intent.PutExtra("ticks", schedule.ScheduledForUtc.Ticks);
        return PendingIntent.GetBroadcast(Context, schedule.Kind == StartKind ? 1 : 0, intent,
            flags | PendingIntentFlags.Immutable);
    }

    private static void CancelAlarm(Guid taskId, string kind)
    {
        var now = DateTimeOffset.UtcNow;
        var placeholder = new ReminderSchedule(taskId, kind, "", null, now, null, 0,
            ReminderScheduleStatus.Active, now);
        var pending = AlarmIntent(placeholder, PendingIntentFlags.NoCreate);
        if (pending is null) return;
        ((AlarmManager?)Context.GetSystemService(Context.AlarmService))?.Cancel(pending);
        pending.Cancel();
    }

    private static PendingIntent OpenTaskIntent(Guid taskId)
    {
        var intent = new Intent(Context, typeof(MainActivity));
        intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        intent.SetData(global::Android.Net.Uri.Parse($"procrastop://task/{taskId:D}"));
        return PendingIntent.GetActivity(Context, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private static string BuildBody(ReminderSchedule x, bool start) => start
        ? x.Description ?? "Llegó tu fecha de inicio. Da el primer paso."
        : x.DueDateUtc.HasValue ? $"Vence: {x.DueDateUtc.Value.ToLocalTime():dd/MM HH:mm}"
        : x.Description ?? "Tienes una tarea pendiente.";

    private static int NotificationId(string kind) => kind == StartKind ? 1 : 0;
    private static DateTimeOffset ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => new DateTimeOffset(value),
        DateTimeKind.Local => new DateTimeOffset(value).ToUniversalTime(),
        _ => new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(value), TimeSpan.Zero)
    };

    private ReminderHistoryItem Event(ReminderSchedule x, ReminderEventStatus status, string? details) =>
        new(Guid.NewGuid(), x.TaskId, x.Kind, status, x.Title, x.ScheduledForUtc.UtcDateTime,
            _timeProvider.GetUtcNow().UtcDateTime, details);
}
