using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Work;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using AndroidApp = Android.App.Application;

namespace ProcraStop.Platforms.Android.Services;

public sealed class AndroidTaskReminderService : ITaskReminderService
{
    private const string StoreName = "task_reminders_v2";
    private const string DueKeyPrefix = "task_";
    private const string StartKeyPrefix = "task_start_";
    private const string AlarmAction = "com.companyname.procrastop.TASK_REMINDER";
    internal const string ChannelId = "task_reminders_v2";
    private static int _permissionScreenOpened;

    private static Context Context => AndroidApp.Context;
    private static ISharedPreferences Store => Context.GetSharedPreferences(StoreName, FileCreationMode.Private)!;

    public Task ScheduleAsync(Guid taskId, string title, string? description, DateTime reminderDate,
        DateTime? dueDate, TaskPriority priority, CancellationToken cancellationToken = default)
        => Schedule(taskId, ReminderKind.DueReminder, title, description, reminderDate, dueDate, priority, cancellationToken);

    public Task ScheduleStartReminderAsync(Guid taskId, string title, string? description,
        DateTime plannedStartDate, TaskPriority priority, CancellationToken cancellationToken = default)
        => Schedule(taskId, ReminderKind.StartReminder, title, description, plannedStartDate, null, priority, cancellationToken);

    private static Task Schedule(Guid taskId, ReminderKind kind, string title, string? description,
        DateTime reminderDate, DateTime? dueDate, TaskPriority priority, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = Key(taskId, kind);
        CancelAlarm(taskId, kind);
        if (kind == ReminderKind.DueReminder)
            CancelLegacyWork(taskId);

        var localTime = reminderDate.Kind == DateTimeKind.Utc ? reminderDate.ToLocalTime() : reminderDate;
        localTime = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
        if (localTime <= DateTime.Now)
        {
            Store.Edit()!.Remove(key).Apply();
            return Task.CompletedTask;
        }

        var record = new ReminderRecord(taskId, kind, title, description, localTime, dueDate, priority);
        Store.Edit()!.PutString(key,
            JsonSerializer.Serialize(record, ReminderJsonContext.Default.ReminderRecord)).Commit();
        Arm(record);
        RequestExactAlarmAccessIfNeeded();
        return Task.CompletedTask;
    }

    public Task CancelDueReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelKind(taskId, ReminderKind.DueReminder);
        CancelLegacyWork(taskId);
        return Task.CompletedTask;
    }

    public Task CancelStartReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelKind(taskId, ReminderKind.StartReminder);
        return Task.CompletedTask;
    }

    public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelKind(taskId, ReminderKind.DueReminder);
        CancelKind(taskId, ReminderKind.StartReminder);
        CancelLegacyWork(taskId);
        return Task.CompletedTask;
    }

    private static void CancelKind(Guid taskId, ReminderKind kind)
    {
        Store.Edit()!.Remove(Key(taskId, kind)).Commit();
        CancelAlarm(taskId, kind);
        ((NotificationManager?)Context.GetSystemService(Context.NotificationService))
            ?.Cancel(taskId.ToString("D"), NotificationId(kind));
    }

    internal static void ReArmAll()
    {
        foreach (var entry in Store.All!)
        {
            if (!entry.Key.StartsWith(DueKeyPrefix, StringComparison.Ordinal) || entry.Value is not string json)
                continue;

            try
            {
                var record = JsonSerializer.Deserialize(json, ReminderJsonContext.Default.ReminderRecord);
                if (record is null) continue;
                if (record.ReminderDate <= DateTime.Now)
                    Store.Edit()!.Remove(entry.Key).Apply();
                else
                    Arm(record);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo restaurar {entry.Key}: {ex}");
            }
        }
    }

    internal static void Deliver(Context context, Guid taskId, ReminderKind kind, long expectedTicks)
    {
        var key = Key(taskId, kind);
        var json = Store.GetString(key, null);
        if (json is null) return;
        ReminderRecord? record;
        try { record = JsonSerializer.Deserialize(json, ReminderJsonContext.Default.ReminderRecord); }
        catch (JsonException) { return; }
        if (record is null || record.Kind != kind || record.ReminderDate.Ticks != expectedTicks) return;

        System.Diagnostics.Debug.WriteLine(
            $"Recordatorio {kind} {taskId}: previsto={record.ReminderDate:O}, recibido={DateTime.Now:O}");

        Store.Edit()!.Remove(key).Commit();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
            context.CheckSelfPermission("android.permission.POST_NOTIFICATIONS") != global::Android.Content.PM.Permission.Granted)
            return;

        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null) return;

        var openIntent = OpenTaskIntent(context, taskId);
        var isStartReminder = kind == ReminderKind.StartReminder;
        var body = isStartReminder
            ? string.IsNullOrWhiteSpace(record.Description)
                ? "Llegó tu fecha de inicio. Da el primer paso."
                : record.Description
            : record.DueDate is { } due
                ? $"Vence: {due:dd/MM HH:mm}"
                : string.IsNullOrWhiteSpace(record.Description) ? "Tienes una tarea pendiente." : record.Description;
        var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O
            ? new Notification.Builder(context, ChannelId)
            : new Notification.Builder(context);
        builder.SetSmallIcon(Resource.Drawable.ic_task_reminder)
            .SetContentTitle(isStartReminder ? $"🚀 ¿Empezamos? {record.Title}" : $"⏰ {record.Title}")
            .SetContentText(body)
            .SetAutoCancel(true)
            .SetContentIntent(openIntent)
            .SetCategory(Notification.CategoryReminder)
            .SetVisibility(NotificationVisibility.Private)
            .SetPriority((int)NotificationPriority.High);
        manager.Notify(taskId.ToString("D"), NotificationId(kind), builder.Build());
    }

    private static void Arm(ReminderRecord record)
    {
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        if (alarm is null) throw new InvalidOperationException("AlarmManager no está disponible.");
        var pending = AlarmIntent(record.TaskId, record.Kind, record.ReminderDate.Ticks, PendingIntentFlags.UpdateCurrent)!;
        var at = new DateTimeOffset(record.ReminderDate).ToUnixTimeMilliseconds();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarm.CanScheduleExactAlarms())
            alarm.SetAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
        else if (record.Kind == ReminderKind.DueReminder && record.Priority == TaskPriority.High)
            alarm.SetAlarmClock(new AlarmManager.AlarmClockInfo(at, OpenTaskIntent(Context, record.TaskId)), pending);
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            alarm.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
        else
            alarm.SetExact(AlarmType.RtcWakeup, at, pending);
    }

    private static void CancelAlarm(Guid taskId, ReminderKind kind)
    {
        var pending = AlarmIntent(taskId, kind, 0, PendingIntentFlags.NoCreate);
        if (pending is null) return;
        ((AlarmManager?)Context.GetSystemService(Context.AlarmService))?.Cancel(pending);
        pending.Cancel();
    }

    private static PendingIntent? AlarmIntent(Guid taskId, ReminderKind kind, long ticks, PendingIntentFlags flags)
    {
        var intent = new Intent(Context, typeof(TaskReminderReceiver));
        intent.SetAction(AlarmAction);
        intent.SetData(global::Android.Net.Uri.Parse(UriFor(taskId, kind)));
        intent.PutExtra("taskId", taskId.ToString("D"));
        intent.PutExtra("kind", (int)kind);
        intent.PutExtra("ticks", ticks);
        return PendingIntent.GetBroadcast(Context, (int)kind, intent, flags | PendingIntentFlags.Immutable);
    }

    private static PendingIntent OpenTaskIntent(Context context, Guid taskId)
    {
        var intent = new Intent(context, typeof(ProcraStop.Platforms.Android.MainActivity));
        intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        intent.SetData(global::Android.Net.Uri.Parse($"procrastop://task/{taskId:D}"));
        return PendingIntent.GetActivity(context, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private static string Key(Guid taskId, ReminderKind kind) =>
        (kind == ReminderKind.StartReminder ? StartKeyPrefix : DueKeyPrefix) + taskId;

    private static string UriFor(Guid taskId, ReminderKind kind) =>
        kind == ReminderKind.StartReminder
            ? $"procrastop://reminder/start/{taskId:D}"
            : $"procrastop://reminder/{taskId:D}";

    private static int NotificationId(ReminderKind kind) => (int)kind;

    private static void CancelLegacyWork(Guid taskId) =>
        WorkManager.GetInstance(Context).CancelUniqueWork(taskId.ToString());

    private static void RequestExactAlarmAccessIfNeeded()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.S ||
            ((AlarmManager?)Context.GetSystemService(Context.AlarmService))?.CanScheduleExactAlarms() != false ||
            Interlocked.Exchange(ref _permissionScreenOpened, 1) != 0) return;

        var activity = Platform.CurrentActivity;
        if (activity is null) return;
        var intent = new Intent("android.settings.REQUEST_SCHEDULE_EXACT_ALARM");
        intent.SetData(global::Android.Net.Uri.Parse($"package:{Context.PackageName}"));
        try { activity.StartActivity(intent); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"No se pudo abrir el permiso de alarmas: {ex}"); }
    }

    internal enum ReminderKind
    {
        DueReminder = 0,
        StartReminder = 1
    }

    internal sealed record ReminderRecord(Guid TaskId, ReminderKind Kind, string Title, string? Description,
        DateTime ReminderDate, DateTime? DueDate, TaskPriority Priority);
}

[JsonSerializable(typeof(AndroidTaskReminderService.ReminderRecord))]
internal partial class ReminderJsonContext : JsonSerializerContext { }
