
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Tasks.Analytics;
using ProcraStop.Application.Tasks.Commands.ArchiveTask;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.Commands.CreateCourse;
using ProcraStop.Application.Tasks.Commands.CreateTask;
using ProcraStop.Application.Tasks.Commands.DeleteCourse;
using ProcraStop.Application.Tasks.Commands.ToggleSubtask;
using ProcraStop.Application.Tasks.Commands.UpdateCourse;
using ProcraStop.Application.Tasks.Commands.UpdateTask;
using ProcraStop.Application.Tasks.EngineTask.Rules;
using ProcraStop.Application.Tasks.EngineTask.Signals;
using ProcraStop.Application.Tasks.Facade;
using ProcraStop.Application.Tasks.Queries.GetCourses;
using ProcraStop.Application.Tasks.Queries.GetTaskDaySummaries;
using ProcraStop.Application.Tasks.Queries.GetTaskById;
using ProcraStop.Application.Tasks.Queries.GetTasks;
using ProcraStop.Application.Tasks.Queries.SearchTasks;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Application.Tasks
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTasksApplication(
        this IServiceCollection services)
        {

            // CQRS Handlers para Tareas
            services.AddTransient<GetTasksQueryHandler>();
            services.AddTransient<GetTaskDaySummariesQueryHandler>(); // editbycodex: resumen CQRS del calendario.
            services.AddTransient<SearchTasksQueryHandler>(); // editbycodex: handler CQRS para busqueda dinamica.
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
            // signals
            services.AddTransient<TaskSignalsBuilder>();
            // Facade
            services.AddScoped<ITasksFacade, TasksFacade>();
            services.AddScoped<ICoursesFacade, CoursesFacade>();
            // dashboard tasks
            services.AddScoped<TaskAnalyticsService>();
            return services;
        }
    }
}
