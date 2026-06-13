
using ProcraStop.Application.Tasks;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Infrastructure.Tasks;
using ProcraStop.Infrastructure.Tasks.Services;

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
                services.AddSingleton<ITaskReminderService, ProcraStop.Platforms.Android.Services.AndroidTaskReminderService>();
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
