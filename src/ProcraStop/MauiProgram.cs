using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Engine;
using ProcraStop.Core.Decisions.Memory;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Rules.Behavior;
using ProcraStop.Modules.Dashboard.Application.Queries;
using ProcraStop.Modules.Dashboard.Presentation.ViewModels;
using ProcraStop.Modules.Dashboard.Presentation.Views;
using ProcraStop.Modules.Habits.Infraestructure.Registration;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using ProcraStop.Modules.Tasks.Infraestructure.Registration;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
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
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //{
            //    options.UseSqlite(Constants.DatabasePath);
            //});
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(
                    $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "procra.db")}"
                ));
            // core
            builder.Services.AddScoped<DecisionContextBuilder>();
            // Registrar store de decisiones en memoria y el motor de decisiones
            builder.Services.AddSingleton<IDecisionMemoryStore, InMemoryDecisionMemoryStore>();
            builder.Services.AddSingleton<DecisionEngine>();
            // =========================
            // Registro encapsulado del módulo Tasks , Habits
            // =========================
            builder.Services.AddTasksModule();
            builder.Services.AddHabitsModule();
            // =========================
            // Registro del servicio de Snackbar
            // =========================
            builder.Services.AddSingleton<ISnackbarService, SnackbarService>();
           
            //dashboard principal
            builder.Services.AddTransient<GetDashboardDecisionHandler>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DashboardView>();


            var app = builder.Build();

            // =========================
            // Crear la base de datos al iniciar
            // =========================
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            return app;
        }
    }
}