
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Stress.Abstractions;
using ProcraStop.Infrastructure.Stress.Repositories;

namespace ProcraStop.Infrastructure.Stress
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStressInfrastructure(
        this IServiceCollection services)
        {
            // =========================
            // REPOSITORY
            // =========================
            services.AddScoped<IStressRepository, StressRepository>();

            return services;
        }
    }
}
