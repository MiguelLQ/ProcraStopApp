using ProcraStop.Application.Notifications;

namespace ProcraStop.Platforms.Android.Services;

internal static class ReminderRuntime
{
    private static IServiceProvider? _services;

    public static void Initialize(IServiceProvider services) => _services = services;

    public static async Task RecordAsync(ReminderHistoryItem item)
    {
        try
        {
            var repository = _services?.GetService<IReminderHistoryRepository>();
            if (repository is not null)
                await repository.AddAsync(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo registrar el historial del recordatorio: {ex}");
        }
    }
}
