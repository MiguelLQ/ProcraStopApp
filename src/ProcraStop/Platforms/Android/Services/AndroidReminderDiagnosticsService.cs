using Android.App;
using Android.Content;
using Android.OS;
using ProcraStop.Application.Notifications;
using AndroidApp = Android.App.Application;
using AndroidX.Core.App;

namespace ProcraStop.Platforms.Android.Services;

public sealed class AndroidReminderDiagnosticsService : IReminderDiagnosticsService
{
    private readonly IReminderHistoryRepository _history;
    private static Context Context => AndroidApp.Context;

    public AndroidReminderDiagnosticsService(IReminderHistoryRepository history)
    {
        _history = history;
    }

    public static void EnsureChannels()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var manager = (NotificationManager?)Context.GetSystemService(Context.NotificationService);
        if (manager is null) return;

        manager.CreateNotificationChannel(CreateChannel(
            AndroidTaskReminderService.DueChannelId,
            "Vencimientos importantes",
            "Avisos de tareas próximas a vencer",
            NotificationImportance.High));
        manager.CreateNotificationChannel(CreateChannel(
            AndroidTaskReminderService.StartChannelId,
            "Inicio de tareas",
            "Avisos para comenzar una tarea planificada",
            NotificationImportance.High));
        manager.CreateNotificationChannel(CreateChannel(
            AndroidTaskReminderService.SummaryChannelId,
            "Resúmenes",
            "Resúmenes y avisos no urgentes",
            NotificationImportance.Default));
    }

    public Task<ReminderCapabilityStatus> GetStatusAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        EnsureChannels();
        var manager = (NotificationManager?)Context.GetSystemService(Context.NotificationService);
        var runtimePermission = Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu ||
            Context.CheckSelfPermission("android.permission.POST_NOTIFICATIONS") ==
            global::Android.Content.PM.Permission.Granted;
        var notificationsGranted = runtimePermission &&
            NotificationManagerCompat.From(Context).AreNotificationsEnabled();
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        var exactGranted = Build.VERSION.SdkInt < BuildVersionCodes.S || alarm?.CanScheduleExactAlarms() == true;
        var dueChannel = ChannelEnabled(manager, AndroidTaskReminderService.DueChannelId);
        var startChannel = ChannelEnabled(manager, AndroidTaskReminderService.StartChannelId);
        var lockScreen = Build.VERSION.SdkInt < BuildVersionCodes.O ||
            manager?.GetNotificationChannel(AndroidTaskReminderService.DueChannelId)?.LockscreenVisibility == NotificationVisibility.Public;
        var precise = notificationsGranted && exactGranted && dueChannel && startChannel;
        var message = precise
            ? "Recordatorios precisos habilitados."
            : BuildMessage(notificationsGranted, exactGranted, dueChannel && startChannel);

        return Task.FromResult(new ReminderCapabilityStatus(
            notificationsGranted, exactGranted, dueChannel, startChannel, lockScreen, precise, message));
    }

    public async Task<ReminderCapabilityStatus> RequestRequiredAccessAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        EnsureChannels();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            await Permissions.RequestAsync<Permissions.PostNotifications>();

        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && alarm?.CanScheduleExactAlarms() == false)
        {
            var activity = Platform.CurrentActivity;
            if (activity is not null)
            {
                var intent = new Intent("android.settings.REQUEST_SCHEDULE_EXACT_ALARM");
                intent.SetData(global::Android.Net.Uri.Parse($"package:{Context.PackageName}"));
                activity.StartActivity(intent);
            }
        }
        else
        {
            var status = await GetStatusAsync(ct);
            if ((!status.DueChannelEnabled || !status.StartChannelEnabled) && Platform.CurrentActivity is { } activity)
            {
                var intent = new Intent("android.settings.APP_NOTIFICATION_SETTINGS");
                intent.PutExtra("android.provider.extra.APP_PACKAGE", Context.PackageName);
                activity.StartActivity(intent);
            }
        }

        return await GetStatusAsync(ct);
    }

    public Task<IReadOnlyList<ReminderHistoryItem>> GetHistoryAsync(
        int limit = 100,
        CancellationToken ct = default) => _history.GetRecentAsync(limit, ct);

    public async Task ReconcileAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await AndroidTaskReminderService.ReArmAllAsync(recordAudit: true, ct);
    }

    private static NotificationChannel CreateChannel(
        string id, string name, string description, NotificationImportance importance)
    {
        var channel = new NotificationChannel(id, name, importance)
        {
            Description = description,
            LockscreenVisibility = NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        return channel;
    }

    private static bool ChannelEnabled(NotificationManager? manager, string id) =>
        Build.VERSION.SdkInt < BuildVersionCodes.O ||
        manager?.GetNotificationChannel(id)?.Importance != NotificationImportance.None;

    private static string BuildMessage(bool notifications, bool exact, bool channels) =>
        !notifications ? "Falta permitir notificaciones."
        : !exact ? "Falta permitir alarmas y recordatorios exactos."
        : !channels ? "Uno o más canales de recordatorios están desactivados."
        : "Revisa la configuración de recordatorios.";
}
