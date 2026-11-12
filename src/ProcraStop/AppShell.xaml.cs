using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using ProcraStop.Modules.Academic.Presentation.Views;
using ProcraStop.Modules.Habits.Presentation.Views;
using ProcraStop.Modules.StudyTechniques.Presentation.Views;
using ProcraStop.Modules.Tasks.Presentation.Views;
using Font = Microsoft.Maui.Font;

using ProcraStop.Diagnostics;

namespace ProcraStop
{
    public partial class AppShell : Shell
    {
        private readonly PerformanceTracer _performance;
        private long _navigationStartedAt;

        public AppShell(PerformanceTracer performance)
        {
            _performance = performance;
            InitializeComponent();

            // Solo se registran rutas para páginas que NO forman parte de la
            // jerarquía de Shell (ShellContent/Tab/FlyoutItem). Registrar una
            // página que ya es ShellContent con el mismo nombre produce
            // "Ambiguous routes matched for: ..." al navegar.
            RegisterRoute("task-detail", typeof(TaskDetailPage));
            RegisterRoute(nameof(CreateTaskPage), typeof(CreateTaskPage));
            RegisterRoute("edit-task", typeof(EditTaskPage));
            RegisterRoute(nameof(TechniqueRecommendationPage), typeof(TechniqueRecommendationPage));
            RegisterRoute(nameof(StudySessionHistoryPage), typeof(StudySessionHistoryPage));
            RegisterRoute(nameof(AcademicHelpPage), typeof(AcademicHelpPage));
            RegisterRoute(nameof(CreateHabitPage), typeof(CreateHabitPage));
            RegisterRoute("habit-track", typeof(TrackHabitPage));

            var currentTheme = Microsoft.Maui.Controls.Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
            Navigating += OnShellNavigating;
            Navigated += OnShellNavigated;
        }

        private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            // Una excepción dentro de Navigating/Navigated rompe el pipeline de
            // navegación: los toques siguientes dejan de responder.
            try
            {
                _navigationStartedAt = _performance.GetTimestamp();
            }
            catch (Exception exception)
            {
                AppCrashLogger.Log(exception, "AppShell.OnShellNavigating");
            }
        }

        // Registro idempotente: UnRegisterRoute evita excepciones si la ruta
        // ya existe (por ejemplo, al recrear AppShell en hot restart).
        private static void RegisterRoute(string route, Type type)
        {
            Routing.UnRegisterRoute(route);
            Routing.RegisterRoute(route, type);
        }

        public static async Task DisplaySnackbarAsync(string message)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Color.FromArgb("#FF3300"),
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.Yellow,
                CornerRadius = new CornerRadius(0),
                Font = Font.SystemFontOfSize(18),
                ActionButtonFont = Font.SystemFontOfSize(14)
            };

            var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public static async Task DisplayToastAsync(string message)
        {
            // Toast is currently not working in MCT on Windows
            if (OperatingSystem.IsWindows())
                return;

            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            try
            {
                if (Microsoft.Maui.Controls.Application.Current is not { } currentApplication)
                    return;

                currentApplication.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
#if ANDROID
                if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is { } activity)
                    ProcraStop.Platforms.Android.Services.AndroidStatusBarService.ApplyThemeStatusBar(activity);
#endif
            }
            catch (Exception exception)
            {
                AppCrashLogger.Log(exception, "AppShell.ThemeSelectionChanged");
            }
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
#if ANDROID
                if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is { } activity)
                    ProcraStop.Platforms.Android.Services.AndroidStatusBarService.ApplyThemeStatusBar(activity);
#endif

                if (_navigationStartedAt == 0)
                    return;

                var route = e.Current?.Location?.OriginalString ?? "unknown";
                var startedAt = _navigationStartedAt;
                _performance.LogElapsed("Navigation", route, startedAt);
                LogFirstRender(CurrentPage, route, startedAt);
            }
            catch (Exception exception)
            {
                AppCrashLogger.Log(exception, "AppShell.OnShellNavigated");
            }
        }

        private void LogFirstRender(Page? page, string route, long startedAt)
        {
            if (page is null)
                return;

            void LogOnNextUiTurn() => Dispatcher.Dispatch(() =>
                _performance.LogElapsed("Render", route, startedAt));

            if (page.IsLoaded)
            {
                LogOnNextUiTurn();
                return;
            }

            EventHandler? loaded = null;
            loaded = (_, _) =>
            {
                page.Loaded -= loaded;
                LogOnNextUiTurn();
            };
            page.Loaded += loaded;
        }
    }
}
