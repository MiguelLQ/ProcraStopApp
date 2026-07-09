
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Habits.Commands.CreateHabit;
using ProcraStop.Application.Habits.Commands.DeactivateHabit;
using ProcraStop.Application.Habits.Commands.TrackHabit;
using ProcraStop.Application.Habits.EngineHabits.Rules;
using ProcraStop.Application.Habits.Interfaces;
using ProcraStop.Application.Habits.Queries.GetAllHabits;
using ProcraStop.Application.Habits.Queries.GetHabitById;
using ProcraStop.Application.Habits.Queries.GetHabitLogByDate;
using ProcraStop.Application.Habits.Queries.GetHabitSignals;
using ProcraStop.Application.Habits.Queries.GetTodayHabits;
using ProcraStop.Application.Habits.Services;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Application.Habits
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHabitsApplication(
        this IServiceCollection services)
        {
            // reglas engine habits
            services.AddTransient<IDecisionRule, HighDisciplineRule>();
            services.AddTransient<IDecisionRule, LowConsistencyRule>();
            services.AddTransient<IDecisionRule, RecoveryMomentumRule>();
            services.AddTransient<IDecisionRule, HighFailureRule>();
            services.AddTransient<IDecisionRule, MomentumGrowthRule>();
            // CQRS handlers
            services.AddTransient<GetAllHabitsHandler>();
            services.AddTransient<CreateHabitHandler>();
            services.AddTransient<DeactivateHabitHandler>();
            services.AddTransient<GetHabitByIdHandler>();
            services.AddTransient<GetHabitLogByDateHandler>();
            services.AddTransient<TrackHabitHandler>();
            services.AddTransient<GetHabitSignalsHandler>();
            services.AddTransient<GetTodayHabitsHandler>(); // editbycodex: agenda diaria optimizada.
            //servicios
            services.AddTransient<IHabitMetricsService, HabitMetricsService>();

            return services;
        }
    }
}
