using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using ProcraStop.Application.Notifications;
using AndroidApp = Android.App.Application;

namespace ProcraStop.Maui11.Platforms.Android.Services;

public sealed class AndroidReminderDiagnosticsService : IReminderDiagnosticsService
{
    private readonly IReminderHistoryRepository _history;
    private readonly AndroidTaskReminderService _reminders;
    private static Context Context => AndroidApp.Context;

    public AndroidReminderDiagnosticsService(
        IReminderHistoryRepository history,
        AndroidTaskReminderService reminders)
    {
        _history = history;
        _reminders = reminders;
    }

    public static void EnsureChannels()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var manager = (NotificationManager?)Context.GetSystemService(Context.NotificationService);
        if (manager is null) return;
        manager.CreateNotificationChannel(Channel(AndroidTaskReminderService.DueChannelId,
            "Vencimientos importantes", "Avisos de tareas próximas a vencer", NotificationImportance.High));
        manager.CreateNotificationChannel(Channel(AndroidTaskReminderService.StartChannelId,
            "Inicio de tareas", "Avisos para comenzar tareas planificadas", NotificationImportance.High));
        manager.CreateNotificationChannel(Channel(AndroidTaskReminderService.SummaryChannelId,
            "Resúmenes", "Avisos no urgentes", NotificationImportance.Default));
    }

    public Task<ReminderCapabilityStatus> GetStatusAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        EnsureChannels();
        var manager = (NotificationManager?)Context.GetSystemService(Context.NotificationService);
        var runtimePermission = Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu ||
            Context.CheckSelfPermission("android.permission.POST_NOTIFICATIONS") == global::Android.Content.PM.Permission.Granted;
        var notifications = runtimePermission && NotificationManagerCompat.From(Context).AreNotificationsEnabled();
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        var exact = Build.VERSION.SdkInt < BuildVersionCodes.S || alarm?.CanScheduleExactAlarms() == true;
        var due = Enabled(manager, AndroidTaskReminderService.DueChannelId);
        var start = Enabled(manager, AndroidTaskReminderService.StartChannelId);
        var lockScreen = Build.VERSION.SdkInt < BuildVersionCodes.O ||
            manager?.GetNotificationChannel(AndroidTaskReminderService.DueChannelId)?.LockscreenVisibility == NotificationVisibility.Public;
        var precise = notifications && exact && due && start;
        var message = precise ? "Recordatorios precisos habilitados."
            : !notifications ? "Falta permitir notificaciones."
            : !exact ? "Falta permitir alarmas exactas."
            : "Uno o más canales están desactivados.";
        return Task.FromResult(new ReminderCapabilityStatus(notifications, exact, due, start,
            lockScreen, precise, message));
    }

    public async Task<ReminderCapabilityStatus> RequestRequiredAccessAsync(CancellationToken ct = default)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            await Permissions.RequestAsync<Permissions.PostNotifications>();
        var alarm = (AlarmManager?)Context.GetSystemService(Context.AlarmService);
        var activity = Platform.CurrentActivity;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && alarm?.CanScheduleExactAlarms() == false && activity is not null)
        {
            var intent = new Intent("android.settings.REQUEST_SCHEDULE_EXACT_ALARM");
            intent.SetData(global::Android.Net.Uri.Parse($"package:{Context.PackageName}"));
            activity.StartActivity(intent);
        }
        else if (activity is not null && await GetStatusAsync(ct) is { } status &&
                 (!status.DueChannelEnabled || !status.StartChannelEnabled))
        {
            var intent = new Intent("android.settings.APP_NOTIFICATION_SETTINGS");
            intent.PutExtra("android.provider.extra.APP_PACKAGE", Context.PackageName);
            activity.StartActivity(intent);
        }
        return await GetStatusAsync(ct);
    }

    public Task<IReadOnlyList<ReminderHistoryItem>> GetHistoryAsync(int limit = 100, CancellationToken ct = default) =>
        _history.GetRecentAsync(limit, ct);

    public Task ReconcileAsync(CancellationToken ct = default) => _reminders.ReconcileAsync(true, ct);

    private static NotificationChannel Channel(string id, string name, string description, NotificationImportance importance)
    {
        var channel = new NotificationChannel(id, name, importance)
        { Description = description, LockscreenVisibility = NotificationVisibility.Public };
        channel.EnableVibration(true);
        return channel;
    }

    private static bool Enabled(NotificationManager? manager, string id) =>
        Build.VERSION.SdkInt < BuildVersionCodes.O ||
        manager?.GetNotificationChannel(id)?.Importance != NotificationImportance.None;
}
