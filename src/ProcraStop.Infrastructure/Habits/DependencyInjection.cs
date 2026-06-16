

using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Habits.Abstractions;
using ProcraStop.Infrastructure.Habits.Repositories;
using ProcraStop.Modules.Habits.Infraestructure.Repositories;

namespace ProcraStop.Infrastructure.Habits
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHabitsInfrastructure(
        this IServiceCollection services)
        {
            // repositories
            services.AddScoped<IHabitRepository, HabitRepository>();
            services.AddScoped<IHabitLogRepository, HabitLogRepository>();
            return services;
        }
    }
}
