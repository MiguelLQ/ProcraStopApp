using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Application.Commands.ArchiveTask;
using ProcraStop.Modules.Tasks.Application.Commands.CompleteTask;
using ProcraStop.Modules.Tasks.Application.Commands.CreateCourse;
using ProcraStop.Modules.Tasks.Application.Commands.CreateTask;
using ProcraStop.Modules.Tasks.Application.Commands.DeleteCourse;
using ProcraStop.Modules.Tasks.Application.Commands.UpdateCourse;
using ProcraStop.Modules.Tasks.Application.Facade;
using ProcraStop.Modules.Tasks.Application.Queries.GetCourses;
using ProcraStop.Modules.Tasks.Application.Queries.GetTaskById;
using ProcraStop.Modules.Tasks.Application.Queries.GetTasks;
using ProcraStop.Modules.Tasks.Infraestructure.Persistence;
using ProcraStop.Modules.Tasks.Infraestructure.Repositories;
using ProcraStop.Modules.Tasks.Infraestructure.Services;
using ProcraStop.Modules.Tasks.Presentation.ViewModels;
using ProcraStop.Modules.Tasks.Presentation.ViewModels.CoursesViewModels;
using ProcraStop.Modules.Tasks.Presentation.Views;
using ProcraStop.Modules.Tasks.Presentation.Views.CoursesView;


namespace ProcraStop.Modules.Tasks.Infraestructure.Registration
{
    public static class TasksModuleServiceRegistration
    {
        public static IServiceCollection AddTasksModule(this IServiceCollection services)
        {
            // Repositorio real con EF Core
            services.AddScoped<ITaskRepository, TaskRepository>();
            // Repository para los cursos
            services.AddScoped<ICourseRepository, CourseRepository>();
            // servicio de notificaciones
#if ANDROID
            services.AddSingleton<ITaskReminderService, ProcraStop.Platforms.Android.Services.AndroidTaskReminderService>();
        #else
        services.AddSingleton<ITaskReminderService, TaskReminderService>();
        #endif
            // Repository de prueba en la memoria
            //services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

            // CQRS Handlers para Tareas
            services.AddTransient<GetTasksQueryHandler>();
            services.AddTransient<CreateTaskCommandHandler>();
            services.AddTransient<CompleteTaskCommandHandler>();
            services.AddTransient<ArchiveTaskCommandHandler>();
            services.AddTransient<GetTaskByIdQueryHandler>();
            // CQRS handlers para cursos
            services.AddTransient<UpdateCourseCommandHandler>();
            services.AddTransient<CreateCourseCommandHandler>();
            services.AddTransient<DeleteCourseCommandHandler>();
            services.AddTransient<GetCoursesQueryHandler>();
            // Facade
            services.AddScoped<ITasksFacade, TasksFacade>();
            services.AddScoped<ICoursesFacade, CoursesFacade>();
            // ViewModels
            services.AddTransient<TasksPageViewModel>();
            services.AddTransient<CreateTaskPageViewModel>();
            services.AddTransient<CoursesPageViewModel>();
            services.AddTransient<TaskDetailViewModel>();
            //// Views
            services.AddTransient<TasksPage>();
            services.AddTransient<CreateTaskPage>();
            services.AddTransient<CoursesPage>();
            services.AddTransient<TaskDetailPage>();
            // 
            services.AddScoped<ITaskReadService, TaskReadService>();

            return services;
        }
    }
}
