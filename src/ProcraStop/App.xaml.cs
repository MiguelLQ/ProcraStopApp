using Microsoft.Extensions.DependencyInjection;

using ProcraStop.Diagnostics;

namespace ProcraStop
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly PerformanceTracer _performance;
        private readonly long _appStartedAt;

        public App(PerformanceTracer performance)
        {
            _performance = performance;
            _appStartedAt = performance.GetTimestamp();
            NavigationSmokeTest.Detect();
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell(_performance);
            EventHandler? loaded = null;
            loaded = (_, _) =>
            {
                shell.Loaded -= loaded;
                Dispatcher.Dispatch(() =>
                    _performance.LogElapsed("Render", "First application frame", _appStartedAt));

                // Diagnóstico opt-in: recorre todas las rutas y escribe el
                // resultado paso a paso en procrastop-crash.log.
                if (NavigationSmokeTest.Enabled)
                    _ = RunNavigationSmokeTestAsync();
            };
            shell.Loaded += loaded;
            return new Window(shell);
        }

        private static async Task RunNavigationSmokeTestAsync()
        {
            try
            {
                await NavigationSmokeTest.RunAsync();
            }
            catch (Exception exception)
            {
                AppCrashLogger.Log(exception, "App.NavigationSmokeTest");
            }
        }
    }
}
