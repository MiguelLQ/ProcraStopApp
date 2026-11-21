using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Work;
using Java.Util.Concurrent;
using ProcraStop.Maui11.Platforms.Android.Services;
using ProcraStop.Maui11.Platforms.Android.Workers;

namespace ProcraStop.Maui11;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        AndroidReminderDiagnosticsService.EnsureChannels();
        _ = ReminderRuntime.GetRequired<AndroidTaskReminderService>().ReconcileAsync(false);

        var work = new PeriodicWorkRequest.Builder(
            Java.Lang.Class.FromType(typeof(ReminderReconciliationWorker)),
            12,
            TimeUnit.Hours).Build();
        WorkManager.GetInstance(this).EnqueueUniquePeriodicWork(
            ReminderReconciliationWorker.UniqueWorkName,
            ExistingPeriodicWorkPolicy.Keep,
            work);
    }
}
