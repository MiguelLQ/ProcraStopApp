

using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Infrastructure.Habits
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHabitsInfrastructure(
        this IServiceCollection services)
        {
            return services;
        }
    }
}
