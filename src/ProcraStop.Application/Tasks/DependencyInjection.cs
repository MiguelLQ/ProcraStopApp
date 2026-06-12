
using Microsoft.Extensions.DependencyInjection;

namespace ProcraStop.Application.Tasks
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTasksApplication(
        this IServiceCollection services)
        {
            return services;
        }
    }
}
