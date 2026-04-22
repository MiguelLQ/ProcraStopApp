
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Application.EngineHabits.Rules;
using ProcraStop.Modules.Habits.Application.Interfaces;
using ProcraStop.Modules.Habits.Application.Queries.GetHabitSignals;
using ProcraStop.Modules.Habits.Application.Services;
using ProcraStop.Modules.Habits.Infraestructure.Repositories;

namespace ProcraStop.Modules.Habits.Infraestructure.Registration
{
    public static class HabitsModuleServiceRegistration
    {
        public static IServiceCollection AddHabitsModule(this IServiceCollection services)
        {
            // habits por ahora aca
            services.AddTransient<IHabitMetricsService, HabitMetricsService>();
            services.AddScoped<IHabitRepository, HabitRepository>();
            services.AddScoped<IHabitLogRepository, HabitLogRepository>();
            services.AddTransient<GetHabitSignalsHandler>();
            // reglas engine habits
            services.AddTransient<IDecisionRule, HighDisciplineRule>();
            services.AddTransient<IDecisionRule, LowConsistencyRule>();
            services.AddTransient<IDecisionRule, RecoveryMomentumRule>();

            return services;
        }
    }
}
