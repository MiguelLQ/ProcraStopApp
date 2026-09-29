using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Work;
using ProcraStop.Application.Tasks.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using AndroidApp = Android.App.Application;

namespace ProcraStop.Platforms.Android.Services;

public sealed class AndroidTaskReminderService : ITaskReminderService
{
    private const string StoreName = "task_reminders_v2";
    private const string KeyPrefix = "task_";
    private const string PermissionPromptKey = "exact_alarm_permission_prompted";
    private const string AlarmAction = "com.companyname.procrastop.TASK_REMINDER";
    internal const string ChannelId = "task_reminders_v2";

    private static Context Context => AndroidApp.Context;
    private static ISharedPreferences Store => Context.GetSharedPreferences(StoreName, FileCreationMode.Private)!;

    public Task ScheduleAsync(Guid taskId, string title, string? description, DateTime reminderDate,
        DateTime? dueDate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancelAlarm(taskId);
        CancelLegacyWork(taskId);

        var localTime = reminderDate.Kind == DateTimeKind.Utc ? reminderDate.ToLocalTime() : reminderDate;
        localTime = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
        if (localTime <= DateTime.Now)
        {
            Store.Edit()!.Remove(KeyPrefix + taskId).Apply();
            return Task.CompletedTask;
        }

        var record = new ReminderRecord(taskId, title, description, localTime, dueDate);
        Store.Edit()!.PutString(KeyPrefix + taskId,
            JsonSerializer.Serialize(record, ReminderJsonContext.Default.ReminderRecord)).Commit();
        Arm(record);
        RequestExactAlarmAccessIfNeeded();
        return Task.CompletedTask;
    }

    public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Store.Edit()!.Remove(KeyPrefix + taskId).Commit();
        CancelAlarm(taskId);
        CancelLegacyWork(taskId);
        ((NotificationManager?)Context.GetSystemService(Context.NotificationService))?.Cancel(taskId.ToString("D"), 0);
        return Task.CompletedTask;
    }

    internal static void ReArmAll()
    {
        foreach (var entry in Store.All!)
        {
            if (!entry.Key.StartsWith(KeyPrefix, StringComparison.Ordinal) || entry.Value is not string json)
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

    internal static void Deliver(Context context, Guid taskId, long expectedTicks)
    {
        var json = Store.GetString(KeyPrefix + taskId, null);
        if (json is null) return;
        ReminderRecord? record;
        try { record = JsonSerializer.Deserialize(json, ReminderJsonContext.Default.ReminderRecord); }
        catch (JsonException) { return; }
        if (record is null || record.ReminderDate.Ticks != expectedTicks) return;

        System.Diagnostics.Debug.WriteLine(
            $"Recordatorio {taskId}: previsto={record.ReminderDate:O}, recibido={DateTime.Now:O}");

        Store.Edit()!.Remove(KeyPrefix + taskId).Commit();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
            context.CheckSelfPermission("android.permission.POST_NOTIFICATIONS") != global::Android.Content.PM.Permission.Granted)
            return;

        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null) return;

        var open = new Intent(context, typeof(ProcraStop.Platforms.Android.MainActivity));
        open.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        open.SetData(global::Android.Net.Uri.Parse($"procrastop://task/{taskId:D}"));
        var openIntent = PendingIntent.GetActivity(context, 0, open,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var body = record.DueDate is { } due
            ? $"Vence: {due:dd/MM HH:mm}"
            : string.IsNullOrWhiteSpace(record.Description) ? "Tienes una tarea pendiente." : record.Description;
        var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O
            ? new Notification.Builder(context, ChannelId)
            : new Notification.Builder(context);
        builder.SetSmallIcon(Resource.Drawable.ic_task_reminder)
            .SetContentTitle($"⏰ {record.Title}")
            .SetContentText(body)
            .SetAutoCancel(true)
            .SetContentIntent(openIntent)
            .SetCategory(Notification.CategoryReminder)
            .SetVisibility(NotificationVisibility.Private)
            .SetPriority((int)NotificationPriority.High);
        manager.Notify(taskId.ToString("D"), 0, builder.Build());
    }

    private static void Arm(ReminderRecord record)
    {
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        if (alarm is null) throw new InvalidOperationException("AlarmManager no está disponible.");
        var pending = AlarmIntent(record.TaskId, record.ReminderDate.Ticks, PendingIntentFlags.UpdateCurrent)!;
        var at = new DateTimeOffset(record.ReminderDate).ToUnixTimeMilliseconds();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && !alarm.CanScheduleExactAlarms())
            alarm.SetAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            alarm.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, at, pending);
        else
            alarm.SetExact(AlarmType.RtcWakeup, at, pending);
    }

    private static void CancelAlarm(Guid taskId)
    {
        var pending = AlarmIntent(taskId, 0, PendingIntentFlags.NoCreate);
        if (pending is null) return;
        ((AlarmManager?)Context.GetSystemService(Context.AlarmService))?.Cancel(pending);
        pending.Cancel();
    }

    private static PendingIntent? AlarmIntent(Guid taskId, long ticks, PendingIntentFlags flags)
    {
        var intent = new Intent(Context, typeof(TaskReminderReceiver));
        intent.SetAction(AlarmAction);
        intent.SetData(global::Android.Net.Uri.Parse($"procrastop://reminder/{taskId:D}"));
        intent.PutExtra("taskId", taskId.ToString("D"));
        intent.PutExtra("ticks", ticks);
        return PendingIntent.GetBroadcast(Context, 0, intent, flags | PendingIntentFlags.Immutable);
    }

    private static void CancelLegacyWork(Guid taskId) =>
        WorkManager.GetInstance(Context).CancelUniqueWork(taskId.ToString());

    private static void RequestExactAlarmAccessIfNeeded()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.S ||
            ((AlarmManager?)Context.GetSystemService(Context.AlarmService))?.CanScheduleExactAlarms() != false ||
            Store.GetBoolean(PermissionPromptKey, false)) return;

        var activity = Platform.CurrentActivity;
        if (activity is null) return;
        Store.Edit()!.PutBoolean(PermissionPromptKey, true).Apply();
        var intent = new Intent("android.settings.REQUEST_SCHEDULE_EXACT_ALARM");
        intent.SetData(global::Android.Net.Uri.Parse($"package:{Context.PackageName}"));
        try { activity.StartActivity(intent); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"No se pudo abrir el permiso de alarmas: {ex}"); }
    }

    internal sealed record ReminderRecord(Guid TaskId, string Title, string? Description,
        DateTime ReminderDate, DateTime? DueDate);
}

[JsonSerializable(typeof(AndroidTaskReminderService.ReminderRecord))]
internal partial class ReminderJsonContext : JsonSerializerContext { }
