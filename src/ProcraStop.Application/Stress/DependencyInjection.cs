
using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Application.Stress
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStressApplication(
        this IServiceCollection services)
        {
            return services;
        }
    }
}
