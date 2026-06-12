using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Rules.Behavior;
using ProcraStop.Modules.Tasks.Application.Commands.UpdateTask;
using ProcraStop.Modules.Tasks.Infraestructure.Repositories;
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
            services.AddTransient<ToggleSubtaskCommandHandler>();
            services.AddTransient<UpdateTaskCommandHandler>();
            // CQRS handlers para cursos
            services.AddTransient<UpdateCourseCommandHandler>();
            services.AddTransient<CreateCourseCommandHandler>();
            services.AddTransient<DeleteCourseCommandHandler>();
            services.AddTransient<GetCoursesQueryHandler>();
            
            // reglas de decision engine
            services.AddTransient<IDecisionRule, OverdueOverloadRule>();
            services.AddTransient<IDecisionRule, QuickWinRule>();
            services.AddTransient<IDecisionRule, MomentumRule>();
            services.AddTransient<IDecisionRule, OverPlanningRule>();
            services.AddTransient<IDecisionRule, AvoidingHardTasksRule>();
            // core
            services.AddTransient<IDecisionRule, FakeProductivityRule>();
            services.AddTransient<IDecisionRule, BurnoutRiskRule>();
            services.AddTransient<IDecisionRule, FakeProductivityWithStressRule>();
            services.AddTransient<IDecisionRule, BurnoutWithStressRule>();
            services.AddTransient<IDecisionRule, LowStressHighEfficiencyRule>();
            services.AddTransient<IDecisionRule, OverloadWithStressRule>();
            services.AddTransient<IDecisionRule, StablePerformanceRule>();

            // signals
            services.AddTransient<TaskSignalsBuilder>();
            // Facade
            services.AddScoped<ITasksFacade, TasksFacade>();
            services.AddScoped<ICoursesFacade, CoursesFacade>();
            // ViewModels
            services.AddTransient<TasksPageViewModel>();
            services.AddTransient<CreateTaskPageViewModel>();
            services.AddTransient<CoursesPageViewModel>();
            services.AddTransient<TaskDetailViewModel>();
            services.AddTransient<EditTaskViewModel>();
            services.AddTransient<TaskAnalyticsViewModel>();
            //// Views
            services.AddTransient<TasksPage>();
            services.AddTransient<CreateTaskPage>();
            services.AddTransient<CoursesPage>();
            services.AddTransient<TaskDetailPage>();
            services.AddTransient<EditTaskPage>();
            services.AddTransient<TaskAnalyticsPage>();
            // 
            services.AddScoped<ITaskReadService, TaskReadService>();
            services.AddScoped<TaskAnalyticsService>();

            return services;
        }
    }
}
