
using ProcraStop.Application.Tasks;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Infrastructure.Tasks;
using ProcraStop.Infrastructure.Tasks.Services;
using ProcraStop.Application.Notifications;

namespace ProcraStop.Modules.Tasks
{
    public static class TasksModule
    {
        public static IServiceCollection AddTasksModule(
        this IServiceCollection services)
        {
            services.AddTasksApplication();

            services.AddTasksInfrastructure();
            #if ANDROID
                services.AddSingleton<ProcraStop.Platforms.Android.Services.AndroidTaskReminderService>();
                services.AddSingleton<ITaskReminderService>(provider =>
                    provider.GetRequiredService<ProcraStop.Platforms.Android.Services.AndroidTaskReminderService>());
                services.AddSingleton<IReminderDiagnosticsService,
                    ProcraStop.Platforms.Android.Services.AndroidReminderDiagnosticsService>();
            #else
                services.AddSingleton<
                    ITaskReminderService,
                    TaskReminderService>();
            #endif

            services.AddTasksPresentation();

            return services;
        }
    }
}
