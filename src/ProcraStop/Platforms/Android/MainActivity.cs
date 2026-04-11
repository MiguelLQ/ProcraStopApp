using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ProcraStop.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CreateNotificationChannel();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelId = "default_channel_id";
                var channelName = "Recordatorios de tareas";

                var channel = new NotificationChannel(
                    channelId,
                    channelName,
                    NotificationImportance.High)
                {
                    Description = "Notificaciones de tareas y recordatorios"
                };

                var manager = (NotificationManager)GetSystemService(NotificationService);
                manager.CreateNotificationChannel(channel);
            }
        }
    }
}
