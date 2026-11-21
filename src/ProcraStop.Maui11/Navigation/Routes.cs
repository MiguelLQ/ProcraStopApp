namespace ProcraStop.Maui11.Navigation;

/// <summary>
/// Contratos de rutas de Shell (kebab-case, estables). Fuente única:
/// skill procra-uiux → shell-navigation.md §10.6 (nada de strings mágicos
/// dispersos).
/// </summary>
public static class Routes
{
    // ---- Rutas de jerarquía (ShellContent en AppShell.xaml) ----
    public const string Dashboard = "DashboardPage";
    public const string Tasks = "TasksPage";
    public const string Habits = "HabitsPage";
    public const string Settings = "SettingsPage";

    // ---- Rutas globales (Routing.RegisterRoute; páginas apilables/modales) ----
    public const string TaskEditor = "task-editor";

    public static string CreateTaskEditor(string activityType)
        => $"{TaskEditor}?activityType={Uri.EscapeDataString(activityType)}";

    public static string EditTaskEditor(Guid taskId)
        => $"{TaskEditor}?taskId={taskId:D}";
}
