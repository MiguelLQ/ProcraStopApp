using Android.App;
using Android.Runtime;
using ProcraStop.Platforms.Android.Services;

namespace ProcraStop.Platforms.Android
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            var app = MauiProgram.CreateMauiApp();
            ReminderRuntime.Initialize(app.Services);
            return app;
        }
    }
}
