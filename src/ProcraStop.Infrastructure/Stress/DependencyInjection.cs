
using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Infrastructure.Stress
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStressInfrastructure(
        this IServiceCollection services)
        {
            return services;
        }
    }
}
