

using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Infrastructure.Dashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDashboardInfrastructure(
        this IServiceCollection services)
        {
            return services;
        }
    }
}
