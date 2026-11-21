using Android.App;
using Android.Runtime;
using ProcraStop.Maui11.Platforms.Android.Services;

namespace ProcraStop.Maui11;

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
