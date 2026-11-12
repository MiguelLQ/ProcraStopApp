using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ProcraStop.Core.Decisions;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Diagnostics;
using ProcraStop.Modules.Academic;
using ProcraStop.Modules.Dashboard;
using ProcraStop.Modules.Habits;
using ProcraStop.Modules.Stress;
using ProcraStop.Modules.StudyTechniques;
using ProcraStop.Modules.Tasks;
using ProcraStop.Shared.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
namespace ProcraStop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Debe instalarse antes de crear cualquier vista: captura las
            // excepciones de async void y de tareas no observadas en un log.
            AppCrashLogger.Install();

            var startupStartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()               // ← agregar esta línea
                .ConfigureSyncfusionToolkit()
                .UseLocalNotification()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
                    Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper
                        .AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
                        {
                            handler.PlatformView.SingleSelectionFollowsFocus = false;
                        });
#endif

#if ANDROID
                    // Elimina el underline nativo de Android en Entry
                    Microsoft.Maui.Handlers.EntryHandler.Mapper
                        .AppendToMapping("NoUnderline", (handler, view) =>
                        {
                            handler.PlatformView.BackgroundTintList =
                                Android.Content.Res.ColorStateList.ValueOf(
                                    Android.Graphics.Color.Transparent);
                        });

                    // Elimina el underline nativo de Android en Editor
                    Microsoft.Maui.Handlers.EditorHandler.Mapper
                        .AppendToMapping("NoUnderline", (handler, view) =>
                        {
                            handler.PlatformView.BackgroundTintList =
                                Android.Content.Res.ColorStateList.ValueOf(
                                    Android.Graphics.Color.Transparent);
                        });
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Inter_18pt-Regular.ttf", "InterRegular");
                    fonts.AddFont("Lato-Bold.ttf", "LatoBold");
                    fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                    fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("Manrope-SemiBold.ttf","ManropeSemiBold");
                    fonts.AddFont("Manrope-Regular.ttf", "ManropeRegular");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            // Configuración de la ruta de la base de datos
            var databasePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "procra-v3.db"); // esquema de desarrollo con ayuda academica
            // registro mejorado por modulos
            builder.Services
            .AddPersistence(databasePath)
            .AddCoreDependencyInjection()
            .AddDashboardModule()
            .AddAcademicModule()
            .AddTasksModule()
            .AddStressModule()
            .AddStudyTechniquesModule()
            .AddHabitsModule();
            // =========================
            // Registro del servicio de Snackbar
            // =========================
            builder.Services.AddSingleton<ISnackbarService, SnackbarService>();
            builder.Services.AddSingleton<PerformanceTracer>();

            var app = builder.Build();
            // Inicializar la base de datos al iniciar la aplicación
            var performance = app.Services.GetRequiredService<PerformanceTracer>();
            using (performance.Measure("Startup", "Database initialization"))
            {
                PersistenceDependencyInjection.InitializeDatabase(app.Services);
            }

            performance.LogElapsed("Startup", "MauiProgram.CreateMauiApp", startupStartedAt);
            // 
            return app;
        }
    }
}
