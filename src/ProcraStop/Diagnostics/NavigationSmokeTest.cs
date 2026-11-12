using ProcraStop.Shared.Services;

namespace ProcraStop.Diagnostics;

/// <summary>
/// Prueba de humo de navegación (solo diagnóstico).
///
/// Se activa con la variable de entorno <c>PROCRASTOP_NAV_SMOKE=1</c>. Recorre
/// todas las rutas de la app — primero los tabs absolutos de Shell y después
/// las rutas globales apiladas — y deja una línea en el log de crash por cada
/// paso con el resultado: si la navegación terminó, qué página quedó visible,
/// si su handler existe y si quedó visible (opacity) o en blanco.
///
/// Así se reproduce sin interacción manual el escenario "Shell se queda roto"
/// y el log señala el paso exacto que falla.
/// </summary>
public static class NavigationSmokeTest
{
    public static bool Enabled { get; private set; }

    private static readonly (string Route, string ExpectedPage, bool ResetAfter)[] Steps =
    [
        ("//TasksPage", "TasksPage", false),
        ("//AnalyticsPage", "TaskAnalyticsPage", false),
        ("//StressAnalyticsPage", "StressAnalyticsPage", false),
        ("//SchedulePage", "SchedulePage", false),
        ("//CoursesPage", "CoursesPage", false),
        ("//DashboardView", "DashboardView", false),
        ("//StudyTechniquesPage", "StudyTechniquesPage", false),
        ("//PomodoroPage", "PomodoroPage", false),
        ("//TechniqueLibraryPage", "TechniqueLibraryPage", false),
        ("//HabitListPage", "HabitListPage", false),
        ("//HabitAnalyticsPage", "HabitAnalyticsPage", false),
        ("//HabitDisciplineRankingPage", "HabitDisciplineRankingPage", false),
        ("CreateTaskPage", "CreateTaskPage", true),
        ("task-detail?taskId=00000000-0000-0000-0000-000000000000", "TaskDetailPage", true),
        ("edit-task?taskId=00000000-0000-0000-0000-000000000000", "EditTaskPage", true),
        ("habit-track?habitId=00000000-0000-0000-0000-000000000000", "TrackHabitPage", true),
        ("CreateHabitPage", "CreateHabitPage", true),
        ("TechniqueRecommendationPage", "TechniqueRecommendationPage", true),
        ("StudySessionHistoryPage", "StudySessionHistoryPage", true),
        ("AcademicHelpPage?topic=Prueba", "AcademicHelpPage", true),
        ("//DashboardView", "DashboardView", false),
    ];

    public static void Detect()
    {
        Enabled = Environment.GetEnvironmentVariable("PROCRASTOP_NAV_SMOKE") == "1";
    }

    /// <summary>
    /// Debe invocarse en el hilo de UI después de que la ventana esté cargada.
    /// </summary>
    public static async Task RunAsync()
    {
        if (!Enabled)
            return;

        try
        {
            AppCrashLogger.LogMessage("NAV-SMOKE inicio");
            await Task.Delay(2500); // deja asentar el arranque

            foreach (var (route, expectedPage, resetAfter) in Steps)
            {
                var ok = await ShellNavigation.GoToAsync(route);
                await Task.Delay(1500);
                Describe(route, expectedPage, ok);
                VerifyGestureCommands(expectedPage);

                if (resetAfter)
                {
                    await Task.Delay(500);
                    await ShellNavigation.GoToAsync("//TasksPage");
                    await Task.Delay(1200);
                }
            }

            await ShellNavigation.GoToAsync("//DashboardView");
            AppCrashLogger.LogMessage("NAV-SMOKE fin");

            await RunFlyoutScenarioAsync();
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, "NavigationSmokeTest");
        }
    }

    /// <summary>
    /// Reproduce el flujo que reporta el usuario: abrir el flyout (cambiar de
    /// elemento), entrar a una página apilada, volver atrás y cambiar de
    /// pestaña de destino.
    /// </summary>
    private static async Task RunFlyoutScenarioAsync()
    {
        try
        {
            var shell = Shell.Current;
            if (shell is null)
                return;

            // 1. Cambiar de elemento del flyout (lo que hace el usuario al
            //    tocar una sección del menú lateral).
            foreach (var item in shell.Items.OfType<ShellItem>())
            {
                shell.CurrentItem = item;
                await Task.Delay(1200);
                AppCrashLogger.LogMessage(
                    $"NAV-SMOKE flyout -> item={item.Title} page={shell.CurrentPage?.GetType().Name ?? "null"}");
            }

            // 2. Entrar a una página apilada y volver atrás.
            await ShellNavigation.GoToAsync("//TasksPage");
            await Task.Delay(800);
            await ShellNavigation.GoToAsync("CreateTaskPage");
            await Task.Delay(1200);
            await ShellNavigation.GoBackAsync();
            await Task.Delay(1200);
            AppCrashLogger.LogMessage(
                $"NAV-SMOKE back -> page={shell.CurrentPage?.GetType().Name ?? "null"}");

            // 3. Cambiar de pestaña con una página apilada en la pila.
            await ShellNavigation.GoToAsync("CreateTaskPage");
            await Task.Delay(1000);
            shell.CurrentItem = shell.Items.OfType<ShellItem>().First();
            await Task.Delay(1500);
            AppCrashLogger.LogMessage(
                $"NAV-SMOKE flyout-con-pila -> page={shell.CurrentPage?.GetType().Name ?? "null"}");

            // 4. Cambiar el tema (recrea handlers y bindings).
            if (Microsoft.Maui.Controls.Application.Current is { } app)
            {
                app.UserAppTheme = AppTheme.Dark;
                await Task.Delay(1500);
                app.UserAppTheme = AppTheme.Light;
                await Task.Delay(1500);
            }

            AppCrashLogger.LogMessage(
                $"NAV-SMOKE flyout-fin -> page={shell.CurrentPage?.GetType().Name ?? "null"}");
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, "NavigationSmokeTest.Flyout");
        }
    }

    /// <summary>
    /// Comprueba que los <see cref="TapGestureRecognizer"/> que dependen de un
    /// comando they've resuelto la referencia: un comando en null significa que
    /// la binding no encontró su origen (el bug que provocaba el
    /// NullReferenceException de MAUI).
    /// </summary>
    private static void VerifyGestureCommands(string label)
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
            return;

        var total = 0;
        var withoutCommand = 0;
        var details = new List<string>();
        var visited = new HashSet<object>();

        void Walk(object element)
        {
            if (element is View view)
            {
                foreach (var recognizer in view.GestureRecognizers)
                {
                    if (recognizer is TapGestureRecognizer tap)
                    {
                        total++;

                        if (tap.Command is null)
                        {
                            withoutCommand++;
                            details.Add(view.GetType().Name);
                        }
                    }
                }
            }

            if (element is IVisualTreeElement visual)
            {
                foreach (var child in visual.GetVisualChildren())
                {
                    if (child is not null && visited.Add(child))
                        Walk(child);
                }
            }
        }

        Walk(page);

        AppCrashLogger.LogMessage(
            $"NAV-SMOKE gestures {label}: total={total} sinComando={withoutCommand} " +
            $"detalle=[{string.Join(",", details)}]");
    }

    private static void Describe(string route, string expectedPage, bool ok)    {
        var page = Shell.Current?.CurrentPage;
        var pageName = page?.GetType().Name ?? "null";
        var handlerOk = page?.Handler is not null;
        var opacity = page?.Opacity ?? -1;

        var match = pageName == expectedPage;
        AppCrashLogger.LogMessage(
            $"NAV-SMOKE {route} -> ok={ok} page={pageName} esperada={expectedPage} " +
            $"match={match} handler={handlerOk} opacity={opacity:F2}");
    }
}
