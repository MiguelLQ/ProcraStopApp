using ProcraStop.Diagnostics;

namespace ProcraStop.Shared.UI;

/// <summary>
/// Utilidades para ejecutar trabajo asíncrono disparado desde el ciclo de vida
/// de la UI (eventos <c>async void</c>) sin que una excepción derribe la app.
/// </summary>
public static class SafeUi
{
    /// <summary>
    /// Ejecuta <paramref name="action"/> y registra cualquier error en lugar de
    /// dejarlo como excepción no observada. Devuelve <c>true</c> si terminó bien.
    /// </summary>
    public static async Task<bool> RunAsync(Func<Task> action, string context)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception exception)
        {
            AppCrashLogger.Log(exception, context);
            return false;
        }
    }
}
