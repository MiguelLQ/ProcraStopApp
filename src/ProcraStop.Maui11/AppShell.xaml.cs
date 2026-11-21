using ProcraStop.Maui11.Navigation;

namespace ProcraStop.Maui11;

public partial class AppShell : Shell
{
    private static bool _routesRegistered;

    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    /// <summary>
    /// Registra las rutas globales (páginas apilables que NO viven en la
    /// jerarquía del Shell). El registro es idempotente y jamás duplica rutas
    /// de ShellContent — ver skill procra-uiux → shell-navigation.md §2.
    /// </summary>
    private static void RegisterRoutes()
    {
        if (_routesRegistered)
            return;

        // El Bottom Sheet solo selecciona el tipo. El editor es una ruta
        // global única para creación rápida, opciones avanzadas y edición.
        Routing.RegisterRoute(Routes.TaskEditor, typeof(Views.Tasks.TaskEditorPage));
        _routesRegistered = true;
    }

    // Blindaje de eventos del pipeline de Shell: una excepción dentro de
    // estos handlers rompe la navegación (bug real de la app .NET 10).
    // Se añaden cuando se suscriban.
}
