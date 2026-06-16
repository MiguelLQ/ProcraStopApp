using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ProcraStop.Core.Decisions;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Modules.Dashboard;
using ProcraStop.Modules.Habits;
using ProcraStop.Modules.Stress;
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
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif
            // Configuración de la ruta de la base de datos
            var databasePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "procra.db");
            // registro mejorado por modulos
            builder.Services
            .AddPersistence(databasePath)
            .AddCoreDependencyInjection()
            .AddDashboardModule()
            .AddTasksModule()
            .AddStressModule()
            .AddHabitsModule();
            // =========================
            // Registro del servicio de Snackbar
            // =========================
            builder.Services.AddSingleton<ISnackbarService, SnackbarService>();

            var app = builder.Build();
            // Inicializar la base de datos al iniciar la aplicación
            PersistenceDependencyInjection.InitializeDatabase(app.Services);
            // 
            return app;
        }
    }
}