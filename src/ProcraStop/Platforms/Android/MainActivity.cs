using Android.App;
using Android.Content.PM;
using Android.OS;
using ProcraStop.Platforms.Android.Services;

namespace ProcraStop.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AndroidStatusBarService.ApplyThemeStatusBar(this);
            CreateNotificationChannel();
            AndroidTaskReminderService.ReArmAll();
        }

        protected override void OnResume()
        {
            base.OnResume();
            AndroidStatusBarService.ApplyThemeStatusBar(this);
            AndroidTaskReminderService.ReArmAll();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = AndroidTaskReminderService.ChannelId;
                var channelName = "Recordatorios de tareas";

                var channel = new NotificationChannel(
                    channelId,
                    channelName,
                    NotificationImportance.High)
                {
                    Description = "Notificaciones de tareas y recordatorios",
                    LockscreenVisibility = NotificationVisibility.Private
                };

                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }
    }
}
