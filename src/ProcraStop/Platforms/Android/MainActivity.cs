using Android.App;
using Android.Content.PM;
using Android.OS;
using ProcraStop.Platforms.Android.Services;
using ProcraStop.Platforms.Android.Workers;
using AndroidX.Work;
using Java.Util.Concurrent;

namespace ProcraStop.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AndroidStatusBarService.ApplyThemeStatusBar(this);
            AndroidReminderDiagnosticsService.EnsureChannels();
            _ = AndroidTaskReminderService.ReArmAllAsync();
            EnsureReminderReconciliation();
        }

        protected override void OnResume()
        {
            base.OnResume();
            AndroidStatusBarService.ApplyThemeStatusBar(this);
        }

        private void EnsureReminderReconciliation()
        {
            var request = new PeriodicWorkRequest.Builder(
                    typeof(ReminderReconciliationWorker),
                    12,
                    TimeUnit.Hours)
                .Build();
            WorkManager.GetInstance(this).EnqueueUniquePeriodicWork(
                ReminderReconciliationWorker.UniqueWorkName,
                ExistingPeriodicWorkPolicy.Keep,
                request);
        }
    }
}
