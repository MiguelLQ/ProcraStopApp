using Microsoft.Extensions.Logging;
using ProcraStop.Maui11.ViewModels.Common;
using ProcraStop.Maui11.ViewModels.Dashboard;
using ProcraStop.Maui11.ViewModels.DesignSystem;
using ProcraStop.Maui11.ViewModels.Habits;
using ProcraStop.Maui11.ViewModels.Settings;
using ProcraStop.Maui11.ViewModels.Tasks;
using ProcraStop.Maui11.Views.Dashboard;
using ProcraStop.Maui11.Views.DesignSystem;
using ProcraStop.Maui11.Views.Habits;
using ProcraStop.Maui11.Views.Settings;
using ProcraStop.Maui11.Views.Tasks;

namespace ProcraStop.Maui11;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Design System (skill procra-uiux → design-system.md §3)
                fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                fonts.AddFont("Inter_18pt-Regular.ttf", "InterRegular");
                fonts.AddFont("Manrope-Regular.ttf", "ManropeRegular");
                // Iconografía
                fonts.AddFont("FluentSystemIcons-Regular.ttf", "FluentUI");
                // Respaldo de plantilla
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Páginas de tab (Shell las resuelve por DI vía ContentTemplate).
        // Transient: cada página recibe su ViewModel por constructor.
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TasksPage>();
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<DesignSystemPage>();

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TasksPageViewModel>();
        builder.Services.AddTransient<HabitsPageViewModel>();
        builder.Services.AddTransient<SettingsPageViewModel>();
        builder.Services.AddTransient<AddSheetViewModel>();
        builder.Services.AddTransient<DesignSystemViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
