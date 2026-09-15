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
            };
            shell.Loaded += loaded;
            return new Window(shell);
        }
    }
}
