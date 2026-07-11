using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Academic.Abstractions;
using ProcraStop.Infrastructure.Academic.Repositories;

namespace ProcraStop.Infrastructure.Academic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAcademicInfrastructure(
            this IServiceCollection services)
        {
            services.AddScoped<ICourseRepository, CourseRepository>();

            return services;
        }
    }
}
