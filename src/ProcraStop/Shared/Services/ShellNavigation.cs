using System.Diagnostics;
using ProcraStop.Diagnostics;

namespace ProcraStop.Shared.Services;

/// <summary>
/// Navegación segura sobre <see cref="Shell"/>.
///
/// Centraliza las guardas que antes estaban repartidas (o ausentes) en los
/// ViewModels:
/// 1. <see cref="Shell.Current"/> puede ser <c>null</c> durante el arranque o
///    el cierre.
/// 2. Dos navegaciones simultáneas (doble toque, evento + comando) pueden
///    dejar el pipeline de Shell en un estado inconsistente del que no se
///    recupera: los toques siguientes no hacen nada. Se serializan con una
///    compuerta global.
/// 3. <c>GoToAsync("..")</c> con la pila vacía deja Shell en un estado roto.
/// </summary>
public static class ShellNavigation
{
    private static readonly SemaphoreSlim NavigationGate = new(1, 1);

    public static async Task<bool> GoToAsync(string route, bool animate = true)
    {
        var shell = Shell.Current;
        if (shell is null)
        {
            AppCrashLogger.Log(
                new InvalidOperationException("Shell.Current no está disponible."),
                $"ShellNavigation.GoToAsync({route})");
            return false;
        }

        await NavigationGate.WaitAsync();
        try
        {
            var startedAt = Stopwatch.GetTimestamp();
            await shell.GoToAsync(route, animate);
            AppCrashLogger.LogMessage(
                $"NAV ok route={route} ms={Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds:F0}");
            return true;
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, $"ShellNavigation.GoToAsync({route})");
            return false;
        }
        finally
        {
            NavigationGate.Release();
        }
    }

    public static async Task<bool> GoToAsync(
        string route,
        IDictionary<string, object> parameters,
        bool animate = true)
    {
        var shell = Shell.Current;
        if (shell is null)
        {
            AppCrashLogger.Log(
                new InvalidOperationException("Shell.Current no está disponible."),
                $"ShellNavigation.GoToAsync({route}, params)");
            return false;
        }

        // Política anti-doble-toque: si ya hay una navegación en curso, la
        // segunda petición se descarta en lugar de encolarse. Encolar
        // navegaciones consecutivas es una de las formas en que Shell termina
        // con la pila en un estado del que no se recupera.
        if (!await NavigationGate.WaitAsync(0))
        {
            AppCrashLogger.LogMessage($"NAV omitido (navegación en curso): {route}");
            return false;
        }

        try
        {
            var startedAt = Stopwatch.GetTimestamp();
            await shell.GoToAsync(route, animate, parameters);
            AppCrashLogger.LogMessage(
                $"NAV ok route={route} ms={Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds:F0}");
            return true;
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, $"ShellNavigation.GoToAsync({route}, params)");
            return false;
        }
        finally
        {
            NavigationGate.Release();
        }
    }

    public static async Task<bool> GoBackAsync(bool animate = true)
    {
        var shell = Shell.Current;
        if (shell is null)
            return false;

        // Navegar hacia atrás sin nada que desapilar puede dejar Shell en un
        // estado roto: el siguiente toque no responde. Se ignora la petición.
        var hasPushedPage = shell.Navigation.NavigationStack.Count > 1;
        var hasModal = shell.Navigation.ModalStack.Count > 0;

        if (!hasPushedPage && !hasModal)
        {
            AppCrashLogger.LogMessage("NAV back ignorado: pila de navegación vacía.");
            return false;
        }

        return await GoToAsync("..", animate);
    }
}
