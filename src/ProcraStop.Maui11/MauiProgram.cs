using Microsoft.Extensions.Logging;
using ProcraStop.Application.Academic;
using ProcraStop.Application.Dashboard;
using ProcraStop.Application.Habits;
using ProcraStop.Application.Stress;
using ProcraStop.Application.StudyTechniques;
using ProcraStop.Application.Tasks;
using ProcraStop.Core.Decisions;
using ProcraStop.Infrastructure.Academic;
using ProcraStop.Infrastructure.Dashboard;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Infrastructure.Habits;
using ProcraStop.Infrastructure.Stress;
using ProcraStop.Infrastructure.StudyTechniques;
using ProcraStop.Infrastructure.Tasks;
#if ANDROID
using Android.Content.Res;
using Android.Widget;
using Google.Android.Material.TextField;
using Microsoft.Maui.Handlers;
using ProcraStop.Application.Notifications;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Maui11.Platforms.Android.Services;
#endif
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

#if ANDROID
        ConfigureAndroidMaterialInputs();
#endif

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "procra-v3.db");
        builder.Services
            .AddPersistence(databasePath)
            .AddCoreDependencyInjection()
            .AddDashboardApplication().AddDashboardInfrastructure()
            .AddAcademicApplication().AddAcademicInfrastructure()
            .AddTasksApplication().AddTasksInfrastructure()
            .AddStressApplication().AddStressInfrastructure()
            .AddStudyTechniquesApplication().AddStudyTechniquesInfrastructure()
            .AddHabitsApplication().AddHabitsInfrastructure();

#if ANDROID
        builder.Services.AddSingleton<AndroidTaskReminderService>();
        builder.Services.AddSingleton<ITaskReminderService>(services =>
            services.GetRequiredService<AndroidTaskReminderService>());
        builder.Services.AddSingleton<IReminderDiagnosticsService, AndroidReminderDiagnosticsService>();
#endif

        // Páginas de tab (Shell las resuelve por DI vía ContentTemplate).
        // Transient: cada página recibe su ViewModel por constructor.
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TasksPage>();
        builder.Services.AddTransient<TaskEditorPage>();
        builder.Services.AddTransient<HabitsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<DesignSystemPage>();

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TasksPageViewModel>();
        builder.Services.AddTransient<TaskEditorViewModel>();
        builder.Services.AddTransient<HabitsPageViewModel>();
        builder.Services.AddTransient<SettingsPageViewModel>();
        builder.Services.AddTransient<AddSheetViewModel>();
        builder.Services.AddTransient<DesignSystemViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        PersistenceDependencyInjection.InitializeDatabase(app.Services);
        return app;
    }

#if ANDROID
    private static void ConfigureAndroidMaterialInputs()
    {
        const string mappingName = "ProcraStopMaterialInputColors";

        EntryHandler2.Mapper.AppendToMapping(mappingName,
            static (handler, _) => ApplyMaterialInputColors(handler.PlatformView));
        EditorHandler2.Mapper.AppendToMapping(mappingName,
            static (handler, _) => ApplyMaterialInputColors(handler.PlatformView));
        PickerHandler2.Mapper.AppendToMapping(mappingName,
            static (handler, _) => ApplyMaterialInputColors(handler.PlatformView));
        DatePickerHandler2.Mapper.AppendToMapping(mappingName,
            static (handler, _) => ApplyMaterialInputColors(handler.PlatformView));
        TimePickerHandler2.Mapper.AppendToMapping(mappingName,
            static (handler, _) => ApplyMaterialInputColors(handler.PlatformView));
    }

    private static void ApplyMaterialInputColors(Android.Views.View platformView)
    {
        if (platformView is not TextInputLayout materialInput)
            return;

        var primary = AndroidX.Core.Content.ContextCompat.GetColor(
            materialInput.Context, Resource.Color.ps_primary);
        var outline = AndroidX.Core.Content.ContextCompat.GetColor(
            materialInput.Context, Resource.Color.ps_outline);
        var hint = AndroidX.Core.Content.ContextCompat.GetColor(
            materialInput.Context, Resource.Color.ps_on_surface_variant);

        int[][] states =
        [
            [Android.Resource.Attribute.StateFocused, Android.Resource.Attribute.StateEnabled],
            [-Android.Resource.Attribute.StateEnabled],
            []
        ];

        materialInput.SetBoxStrokeColorStateList(
            new ColorStateList(states, [primary, outline, outline]));
        materialInput.HintTextColor =
            new ColorStateList(states, [primary, hint, hint]);

        if (materialInput.EditText is TextView textView && OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            textView.TextCursorDrawable?.SetTint(new Android.Graphics.Color(primary));
        }
    }
#endif
}
