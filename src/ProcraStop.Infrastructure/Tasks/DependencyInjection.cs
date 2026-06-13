
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Infraestructure.Tasks.Services;
using ProcraStop.Infrastructure.Tasks.Repositories;
using ProcraStop.Infrastructure.Tasks.Services;

namespace ProcraStop.Infrastructure.Tasks
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services)
        {

            // servicio de notificaciones
            services.AddSingleton<ITaskReminderService, TaskReminderService>();
            
            // Repositorio real con EF Core
            services.AddScoped<ITaskRepository, TaskRepository>();
            // Repository para los cursos
            services.AddScoped<ICourseRepository, CourseRepository>();
            // Repository de prueba en la memoria
            //services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
            // para leer tareas
            services.AddScoped<ITaskReadService, TaskReadService>();
            return services;
        }
    }
}
