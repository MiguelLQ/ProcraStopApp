
using ProcraStop.Application.Habits;
using ProcraStop.Infrastructure.Habits;

namespace ProcraStop.Modules.Habits
{
    public static class HabitsModule
    {
        public static IServiceCollection AddHabitsModule(
        this IServiceCollection services)
        {
            services.AddHabitsApplication();

            services.AddHabitsInfrastructure();

            services.AddHabitsPresentation();

            return services;
        }
    }
}
