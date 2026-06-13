
using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Application.Habits
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHabitsApplication(
        this IServiceCollection services)
        {
            return services;
        }
    }
}
