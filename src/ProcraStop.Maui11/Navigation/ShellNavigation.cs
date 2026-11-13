using System.Diagnostics;

namespace ProcraStop.Maui11.Navigation;

/// <summary>
/// Compuerta de navegación Shell (patrón probado en la app .NET 10):
/// serializa navegaciones con un semáforo global y DESCARTA peticiones
/// concurrentes (doble toque) en vez de encolarlas — dos GoToAsync
/// simultáneos dejan el pipeline de Shell en estado irrecuperable.
/// Skill procra-uiux → shell-navigation.md §3.
/// </summary>
public static class ShellNavigation
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    /// <summary>
    /// Navega a la ruta indicada. Devuelve false si la petición fue
    /// descartada (navegación en curso) o si Shell no está disponible.
    /// </summary>
    public static async Task<bool> GoToAsync(string route, bool animate = true)
    {
        // Descarta en vez de encolar: la primera navegación gana.
        if (!await Gate.WaitAsync(0))
        {
            Trace.WriteLine($"[PS-NAV] omitido (navegación en curso): {route}");
            return false;
        }

        try
        {
            var shell = Shell.Current;
            if (shell is null)
                return false;

            await shell.GoToAsync(route, animate);
            Trace.WriteLine($"[PS-NAV] ok: {route}");
            return true;
        }
        catch (Exception exception)
        {
            Trace.WriteLine($"[PS-NAV] error {route}: {exception}");
            return false;
        }
        finally
        {
            Gate.Release();
        }
    }

    /// <summary>
    /// Retrocede solo si existe una página apilada o un modal (nunca
    /// desapila a ciegas).
    /// </summary>
    public static async Task<bool> GoBackAsync()
    {
        var shell = Shell.Current;
        if (shell is null)
            return false;

        var navigation = shell.Navigation;
        if (navigation.NavigationStack.Count > 1 || navigation.ModalStack.Count > 0)
            return await GoToAsync("..");

        return false;
    }
}
