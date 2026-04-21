using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ProcraStop.Core.Decisions.Engine;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using ProcraStop.Modules.Tasks.Infraestructure.Registration;
using ProcraStop.Shared.Services;
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
                    fonts.AddFont("Inter_18pt-SemiBold.ttf", "InterSemiBold");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            // =========================
            // Base de datos SQLite (EF Core)
            // =========================
            builder.Services.AddDbContext<TasksDbContext>(options =>
            {
                options.UseSqlite(Constants.DatabasePath);
            });

            // =========================
            // Registro encapsulado del módulo Tasks
            // =========================
            builder.Services.AddTasksModule();
            // =========================
            // Registro del servicio de Snackbar
            // =========================
            builder.Services.AddSingleton<ISnackbarService, SnackbarService>();
            // core
            builder.Services.AddScoped<DecisionEngine>();

            var app = builder.Build();

            // =========================
            // Crear la base de datos al iniciar
            // =========================
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
                dbContext.Database.EnsureCreated();
            }

            return app;
        }
    }
}