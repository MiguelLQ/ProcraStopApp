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

        // El Bottom Sheet "Agregar" es overlay embebido (Components/Sheets/
        // PSAddSheet), no requiere ruta. Las primeras páginas apilables
        // (task-detail, create-task, ...) se registrarán aquí con las
        // constantes de Routes.
        _routesRegistered = true;
    }

    // Blindaje de eventos del pipeline de Shell: una excepción dentro de
    // estos handlers rompe la navegación (bug real de la app .NET 10).
    // Se añaden cuando se suscriban.
}
