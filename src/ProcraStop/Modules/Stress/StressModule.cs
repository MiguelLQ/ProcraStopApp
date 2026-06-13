
using ProcraStop.Application.Stress;
using ProcraStop.Infrastructure.Stress;

namespace ProcraStop.Modules.Stress
{
    public static class StressModule
    {
        public static IServiceCollection AddStressModule(
        this IServiceCollection services)
        {
            services.AddStressApplication();

            services.AddStressInfrastructure();

            services.AddStressPresentation();

            return services;
        }
    }
}
