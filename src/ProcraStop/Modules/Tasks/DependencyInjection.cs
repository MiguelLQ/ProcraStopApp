
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Modules.Tasks.Presentation.Views;

namespace ProcraStop.Modules.Tasks
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTasksPresentation(
        this IServiceCollection services)
        {
            // ViewModels
            services.AddTransient<TasksPageViewModel>();
            services.AddTransient<CreateTaskPageViewModel>();
            services.AddTransient<TaskDetailViewModel>();
            services.AddTransient<EditTaskViewModel>();
            services.AddTransient<TaskAnalyticsViewModel>();
            //// Views tasks
            services.AddTransient<TasksPage>();
            services.AddTransient<CreateTaskPage>();
            services.AddTransient<TaskDetailPage>();
            services.AddTransient<EditTaskPage>();
            services.AddTransient<TaskAnalyticsPage>();
            return services;
        }
    }
}
