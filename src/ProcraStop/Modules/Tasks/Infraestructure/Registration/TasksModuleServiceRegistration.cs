using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.Commands.ArchiveTask;
using ProcraStop.Modules.Tasks.Application.Commands.CompleteTask;
using ProcraStop.Modules.Tasks.Application.Commands.CreateTask;
using ProcraStop.Modules.Tasks.Application.Facade;
using ProcraStop.Modules.Tasks.Application.Queries.GetTasks;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using ProcraStop.Modules.Tasks.Infraestructure.Repositories;
using ProcraStop.Modules.Tasks.Infraestructure.Services;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Modules.Tasks.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Infraestructure.Registration
{
    public static class TasksModuleServiceRegistration
    {
        public static IServiceCollection AddTasksModule(this IServiceCollection services)
        {
            // Repositorio real con EF Core
            services.AddScoped<ITaskRepository, TaskRepository>();
            // servicio de notificaciones
        #if ANDROID
                    services.AddSingleton<ITaskReminderService, ProcraStop.Platforms.Android.Services.AndroidTaskReminderService>();
        #else
        services.AddSingleton<ITaskReminderService, TaskReminderService>();
        #endif
            // Repository de prueba en la memoria
            //services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

            // CQRS Handlers
            services.AddTransient<GetTasksQueryHandler>();
            services.AddTransient<CreateTaskCommandHandler>();
            services.AddTransient<CompleteTaskCommandHandler>();
            services.AddTransient<ArchiveTaskCommandHandler>();

            // Facade
            services.AddScoped<ITasksFacade, TasksFacade>();

            // ViewModels
            services.AddTransient<TasksPageViewModel>();
            services.AddTransient<CreateTaskPageViewModel>();

            //// Views
            services.AddTransient<TasksPage>();
            services.AddTransient<CreateTaskPage>();

            return services;
        }
    }
}
