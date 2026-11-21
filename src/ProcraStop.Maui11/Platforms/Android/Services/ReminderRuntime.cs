namespace ProcraStop.Maui11.Platforms.Android.Services;

internal static class ReminderRuntime
{
    private static IServiceProvider? _services;
    public static void Initialize(IServiceProvider services) => _services = services;
    public static T GetRequired<T>() where T : notnull
    {
        var services = _services ?? throw new InvalidOperationException("Servicios Android no inicializados.");
        return services.GetRequiredService<T>();
    }
}
