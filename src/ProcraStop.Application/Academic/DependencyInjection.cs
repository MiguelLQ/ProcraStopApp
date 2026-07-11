using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Academic.Commands.CreateCourse;
using ProcraStop.Application.Academic.Commands.DeleteCourse;
using ProcraStop.Application.Academic.Commands.UpdateCourse;
using ProcraStop.Application.Academic.Facade;
using ProcraStop.Application.Academic.Queries.GetCourses;

namespace ProcraStop.Application.Academic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAcademicApplication(
            this IServiceCollection services)
        {
            services.AddTransient<GetCoursesQueryHandler>();
            services.AddTransient<CreateCourseCommandHandler>();
            services.AddTransient<UpdateCourseCommandHandler>();
            services.AddTransient<DeleteCourseCommandHandler>();
            services.AddScoped<ICoursesFacade, CoursesFacade>();

            return services;
        }
    }
}
