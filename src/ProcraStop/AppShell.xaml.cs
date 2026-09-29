using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using ProcraStop.Modules.Academic.Presentation.Views;
using ProcraStop.Modules.Dashboard.Presentation.Views;
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
            // registrar ruta 
            Routing.RegisterRoute("task-detail", typeof(TaskDetailPage));
            Routing.RegisterRoute(nameof(CreateTaskPage), typeof(CreateTaskPage));
            Routing.RegisterRoute(nameof(CoursesPage), typeof(CoursesPage));
            
            Routing.RegisterRoute("edit-task", typeof(EditTaskPage));
            Routing.RegisterRoute(nameof(TaskAnalyticsPage), typeof(TaskAnalyticsPage));
            Routing.RegisterRoute(nameof(DashboardView), typeof(DashboardView));
            Routing.RegisterRoute(nameof(SchedulePage), typeof(SchedulePage));
            Routing.RegisterRoute(nameof(StudyTechniquesPage), typeof(StudyTechniquesPage));
            Routing.RegisterRoute(nameof(PomodoroPage), typeof(PomodoroPage));
            Routing.RegisterRoute(nameof(TechniqueRecommendationPage), typeof(TechniqueRecommendationPage));
            Routing.RegisterRoute(nameof(TechniqueLibraryPage), typeof(TechniqueLibraryPage));
            Routing.RegisterRoute(nameof(StudySessionHistoryPage), typeof(StudySessionHistoryPage));
            Routing.RegisterRoute(nameof(AcademicHelpPage), typeof(AcademicHelpPage));
            Routing.RegisterRoute(nameof(CreateHabitPage), typeof(CreateHabitPage));
            Routing.RegisterRoute(nameof(HabitListPage), typeof(HabitListPage));
            Routing.RegisterRoute(nameof(HabitAnalyticsPage), typeof(HabitAnalyticsPage));
            Routing.RegisterRoute(nameof(HabitDisciplineRankingPage), typeof(HabitDisciplineRankingPage));
            Routing.RegisterRoute("habit-track", typeof(TrackHabitPage));
            var currentTheme = Microsoft.Maui.Controls.Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
            Navigating += OnShellNavigating;
            Navigated += OnShellNavigated;
        }

        private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            _navigationStartedAt = _performance.GetTimestamp();
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
            Microsoft.Maui.Controls.Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
#if ANDROID
            if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is { } activity)
                ProcraStop.Platforms.Android.Services.AndroidStatusBarService.ApplyThemeStatusBar(activity);
#endif
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
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
