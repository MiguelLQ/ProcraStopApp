using System.Text;
using Microsoft.Maui.Storage;

namespace ProcraStop.Diagnostics;

/// <summary>
/// Captura centralizada de excepciones no controladas.
///
/// En MAUI una excepción lanzada desde un <c>async void</c> (por ejemplo
/// <c>OnAppearing</c>, un evento de messenger o una lambda de
/// <c>PropertyChanged</c>) no se puede observar desde el llamador y termina
/// derribando el proceso. Este logger escribe la traza completa en un archivo
/// dentro de <see cref="FileSystem.AppDataDirectory"/> para poder diagnosticar
/// el fallo incluso cuando el depurador no está conectado.
/// </summary>
public static class AppCrashLogger
{
    private const string FileName = "procrastop-crash.log";
    private const long MaxBytes = 1_000_000; // 1 MB: evita crecer sin control.

    private static readonly object Gate = new();
    private static bool _installed;

    public static string LogPath => Path.Combine(FileSystem.AppDataDirectory, FileName);

    /// <summary>
    /// Engancha los tres canales de excepción no observada. Es idempotente.
    /// Debe llamarse lo antes posible dentro de <c>MauiProgram.CreateMauiApp</c>.
    /// </summary>
    public static void Install()
    {
        if (_installed)
            return;

        _installed = true;

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log(args.ExceptionObject as Exception, "AppDomain.UnhandledException");

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log(args.Exception, "TaskScheduler.UnobservedTaskException");
            // Marcar como observada evita que el runtime finalice el proceso
            // por una tarea de fondo que ya registramos.
            args.SetObserved();
        };

#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, args) =>
            Log(args.Exception, "AndroidEnvironment.UnhandledExceptionRaiser");
#endif
    }

    /// <summary>
    /// Escribe una línea informativa (sin excepción) en el log. Se usa para
    /// trazas de diagnóstico como la navegación realizada.
    /// </summary>
    public static void LogMessage(string message)
    {
        try
        {
            var entry = new StringBuilder()
                .Append('[').Append(DateTimeOffset.Now.ToString("O")).Append("] ")
                .AppendLine(message)
                .ToString();

            lock (Gate)
            {
                RotateIfNeeded();
                File.AppendAllText(LogPath, entry, Encoding.UTF8);
            }

            System.Diagnostics.Debug.WriteLine(entry);
        }
        catch
        {
            // El logger jamás debe convertirse en una nueva causa de fallo.
        }
    }

    /// <summary>
    /// Registra una excepción. Nunca propaga errores propios del logger.
    /// </summary>
    public static void Log(Exception? exception, string? context = null)
    {
        if (exception is null)
            return;

        try
        {
            var entry = new StringBuilder()
                .Append('[').Append(DateTimeOffset.Now.ToString("O")).Append("] ")
                .AppendLine(context ?? "Unhandled")
                .AppendLine(exception.ToString())
                .AppendLine(new string('-', 80))
                .ToString();

            lock (Gate)
            {
                RotateIfNeeded();
                File.AppendAllText(LogPath, entry, Encoding.UTF8);
            }

            System.Diagnostics.Debug.WriteLine(entry);
            Console.WriteLine(entry);
        }
        catch
        {
            // El logger jamás debe convertirse en una nueva causa de fallo.
        }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            var info = new FileInfo(LogPath);
            if (info.Exists && info.Length > MaxBytes)
                info.Delete();
        }
        catch
        {
            // Si no se puede rotar, se sigue escribiendo: no es crítico.
        }
    }
}
