using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Work;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using AndroidApp = Android.App.Application;
using ProcraStop.Application.Notifications;

namespace ProcraStop.Platforms.Android.Services;

public sealed class AndroidTaskReminderService : ITaskReminderService
{
    private const string StoreName = "task_reminders_v2";
    private const string DueKeyPrefix = "task_";
    private const string StartKeyPrefix = "task_start_";
    private const string AlarmAction = "com.companyname.procrastop.TASK_REMINDER";
    internal const string DueChannelId = "task_due_reminders_v3";
    internal const string StartChannelId = "task_start_reminders_v3";
    internal const string SummaryChannelId = "task_summary_v1";
    private static int _permissionScreenOpened;

    private static Context Context => AndroidApp.Context;
    private static ISharedPreferences Store => Context.GetSharedPreferences(StoreName, FileCreationMode.Private)!;

    public Task ScheduleAsync(Guid taskId, string title, string? description, DateTime reminderDate,
        DateTime? dueDate, TaskPriority priority, CancellationToken cancellationToken = default)
        => Schedule(taskId, ReminderKind.DueReminder, title, description, reminderDate, dueDate, priority, cancellationToken);

    public Task ScheduleStartReminderAsync(Guid taskId, string title, string? description,
        DateTime plannedStartDate, TaskPriority priority, CancellationToken cancellationToken = default)
        => Schedule(taskId, ReminderKind.StartReminder, title, description, plannedStartDate, null, priority, cancellationToken);

    private static async Task Schedule(Guid taskId, ReminderKind kind, string title, string? description,
        DateTime reminderDate, DateTime? dueDate, TaskPriority priority, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = Key(taskId, kind);
        CancelAlarm(taskId, kind);
        if (kind == ReminderKind.DueReminder)
            CancelLegacyWork(taskId);

        var localTime = reminderDate.Kind == DateTimeKind.Utc ? reminderDate.ToLocalTime() : reminderDate;
        localTime = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
        var record = new ReminderRecord(taskId, kind, title, description, localTime, dueDate, priority);
        if (localTime <= DateTime.Now)
        {
            Store.Edit()!.Remove(key).Apply();
            await AuditAsync(record, ReminderEventStatus.Failed, "La fecha del recordatorio ya pasó");
            return;
        }

        Store.Edit()!.PutString(key,
            JsonSerializer.Serialize(record, ReminderJsonContext.Default.ReminderRecord)).Commit();
        bool exact;
        try
        {
            exact = Arm(record);
        }
        catch (Exception ex)
        {
            await AuditAsync(record, ReminderEventStatus.Failed, ex.Message);
            throw;
        }
        await AuditAsync(
            record,
            exact ? ReminderEventStatus.ScheduledExact : ReminderEventStatus.ScheduledInexact,
            exact ? "AlarmManager exacto" : "Permiso exacto ausente; programación aproximada");
        RequestExactAlarmAccessIfNeeded();
    }

    public async Task CancelDueReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await CancelKindAsync(taskId, ReminderKind.DueReminder);
        CancelLegacyWork(taskId);
    }

    public async Task CancelStartReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await CancelKindAsync(taskId, ReminderKind.StartReminder);
    }

    public async Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await CancelKindAsync(taskId, ReminderKind.DueReminder);
        await CancelKindAsync(taskId, ReminderKind.StartReminder);
        CancelLegacyWork(taskId);
    }

    private static async Task CancelKindAsync(Guid taskId, ReminderKind kind)
    {
        var record = ReadRecord(taskId, kind);
        Store.Edit()!.Remove(Key(taskId, kind)).Commit();
        CancelAlarm(taskId, kind);
        ((NotificationManager?)Context.GetSystemService(Context.NotificationService))
            ?.Cancel(taskId.ToString("D"), NotificationId(kind));
        if (record is not null)
            await AuditAsync(record, ReminderEventStatus.Cancelled, "Cancelado por la aplicación");
    }

    internal static async Task ReArmAllAsync(
        bool recordAudit = false,
        CancellationToken ct = default)
    {
        foreach (var entry in Store.All!)
        {
            ct.ThrowIfCancellationRequested();
            if (!entry.Key.StartsWith(DueKeyPrefix, StringComparison.Ordinal) || entry.Value is not string json)
                continue;

            try
            {
                var record = JsonSerializer.Deserialize(json, ReminderJsonContext.Default.ReminderRecord);
                if (record is null) continue;
                if (record.ReminderDate <= DateTime.Now)
                    Store.Edit()!.Remove(entry.Key).Apply();
                else
                {
                    var exact = Arm(record);
                    if (recordAudit)
                        await AuditAsync(
                            record,
                            ReminderEventStatus.Reconciled,
                            exact ? "Reprogramado exacto" : "Reprogramado aproximado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo restaurar {entry.Key}: {ex}");
            }
        }
    }

    internal static async Task DeliverAsync(
        Context context,
        Guid taskId,
        ReminderKind kind,
        long expectedTicks)
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
        {
            await AuditAsync(record, ReminderEventStatus.PermissionMissing, "POST_NOTIFICATIONS no autorizado");
            return;
        }

        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null)
        {
            await AuditAsync(record, ReminderEventStatus.Failed, "NotificationManager no disponible");
            return;
        }

        var openIntent = OpenTaskIntent(context, taskId);
        var isStartReminder = kind == ReminderKind.StartReminder;
        var body = isStartReminder
            ? string.IsNullOrWhiteSpace(record.Description)
                ? "Llegó tu fecha de inicio. Da el primer paso."
                : record.Description
            : record.DueDate is { } due
                ? $"Vence: {due:dd/MM HH:mm}"
                : string.IsNullOrWhiteSpace(record.Description) ? "Tienes una tarea pendiente." : record.Description;
        var channelId = isStartReminder ? StartChannelId : DueChannelId;
        var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O
            ? new Notification.Builder(context, channelId)
            : new Notification.Builder(context);
        builder.SetSmallIcon(Resource.Drawable.ic_task_reminder)
            .SetContentTitle(isStartReminder ? $"🚀 ¿Empezamos? {record.Title}" : $"⏰ {record.Title}")
            .SetContentText(body)
            .SetAutoCancel(true)
            .SetContentIntent(openIntent)
            .SetCategory(Notification.CategoryReminder)
            .SetVisibility(NotificationVisibility.Public)
            .SetPriority((int)NotificationPriority.High);
        manager.Notify(taskId.ToString("D"), NotificationId(kind), builder.Build());
        await AuditAsync(record, ReminderEventStatus.Delivered,
            $"Entregado con desfase de {(DateTime.Now - record.ReminderDate).TotalSeconds:F0} segundos");
    }

    private static bool Arm(ReminderRecord record)
    {
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        if (alarm is null) throw new InvalidOperationException("AlarmManager no está disponible.");
        var pending = AlarmIntent(record.TaskId, record.Kind, record.ReminderDate.Ticks, PendingIntentFlags.UpdateCurrent)!;
        var at = new DateTimeOffset(record.ReminderDate).ToUnixTimeMilliseconds();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarm.CanScheduleExactAlarms())
        {
            alarm.SetAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
            return false;
        }
        else if (record.Kind == ReminderKind.DueReminder && record.Priority == TaskPriority.High)
            alarm.SetAlarmClock(new AlarmManager.AlarmClockInfo(at, OpenTaskIntent(Context, record.TaskId)), pending);
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            alarm.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
        else
            alarm.SetExact(AlarmType.RtcWakeup, at, pending);
        return true;
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

    private static ReminderRecord? ReadRecord(Guid taskId, ReminderKind kind)
    {
        var json = Store.GetString(Key(taskId, kind), null);
        if (json is null) return null;
        try { return JsonSerializer.Deserialize(json, ReminderJsonContext.Default.ReminderRecord); }
        catch (JsonException) { return null; }
    }

    private static Task AuditAsync(
        ReminderRecord record,
        ReminderEventStatus status,
        string? details) => ReminderRuntime.RecordAsync(new ReminderHistoryItem(
            Guid.NewGuid(),
            record.TaskId,
            record.Kind.ToString(),
            status,
            record.Title,
            TimeZoneInfo.ConvertTimeToUtc(record.ReminderDate),
            DateTime.UtcNow,
            details));

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
