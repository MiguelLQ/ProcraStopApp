
using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Application.Dashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDashboardApplication(
       this IServiceCollection services)
        {
            return services;
        }
    }
}
