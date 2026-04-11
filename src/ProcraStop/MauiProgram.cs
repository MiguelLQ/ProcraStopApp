
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using ProcraStop.Modules.Tasks.Infraestructure.Registration;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Modules.Tasks.Presentation.Views;
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
    				Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
    				{
    					handler.PlatformView.SingleSelectionFollowsFocus = false;
    				});

 
#endif
                })
                .ConfigureFonts(fonts =>
                {
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
            // (Aquí van repo, handlers, facade, VMs, Views del módulo)
            // =========================
            builder.Services.AddTasksModule();

            builder.Services.AddTransientWithShellRoute<TaskListPage, TaskListViewModel>("tasks");
            builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailViewModel>("TaskDetailPage");
            builder.Services.AddTransientWithShellRoute<AddTaskPage , AddTaskViewModel>("AddTaskPage");
            builder.Services.AddTransientWithShellRoute<CalendarPage, CalendarViewModel>("CalendarPage");

            var app = builder.Build();
            // =========================
            // Crear la base de datos y tabla Tasks al iniciar
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
