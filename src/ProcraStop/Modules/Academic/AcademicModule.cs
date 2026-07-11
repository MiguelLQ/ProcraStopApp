using ProcraStop.Application.Academic;
using ProcraStop.Infrastructure.Academic;

namespace ProcraStop.Modules.Academic
{
    public static class AcademicModule
    {
        public static IServiceCollection AddAcademicModule(
            this IServiceCollection services)
        {
            services.AddAcademicApplication();
            services.AddAcademicInfrastructure();
            services.AddAcademicPresentation();

            return services;
        }
    }
}
