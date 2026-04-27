
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Habits.Application.Abstractions;
using ProcraStop.Modules.Habits.Application.Commands.CreateHabit;
using ProcraStop.Modules.Habits.Application.Commands.TrackHabit;
using ProcraStop.Modules.Habits.Application.EngineHabits.Rules;
using ProcraStop.Modules.Habits.Application.Interfaces;
using ProcraStop.Modules.Habits.Application.Queries.GetAllHabits;
using ProcraStop.Modules.Habits.Application.Queries.GetHabitById;
using ProcraStop.Modules.Habits.Application.Queries.GetHabitLogByDate;
using ProcraStop.Modules.Habits.Application.Queries.GetHabitSignals;
using ProcraStop.Modules.Habits.Application.Services;
using ProcraStop.Modules.Habits.Infraestructure.Repositories;
using ProcraStop.Modules.Habits.Presentation.ViewModels;
using ProcraStop.Modules.Habits.Presentation.Views;

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
            services.AddTransient<IDecisionRule, HighFailureRule>();
            services.AddTransient<IDecisionRule, MomentumGrowthRule>();
            // views y view models
            services.AddTransient<CreateHabitHandler>();
            services.AddTransient<CreateHabitViewModel>();
            services.AddTransient<CreateHabitPage>();
            services.AddTransient<GetAllHabitsHandler>();
            services.AddTransient<HabitListViewModel>();
            services.AddTransient<HabitListPage>();
            services.AddTransient<TrackHabitViewModel>();
            services.AddTransient<TrackHabitPage>();

            services.AddTransient<GetHabitByIdHandler>();
            services.AddTransient<GetHabitLogByDateHandler>();
            services.AddTransient<TrackHabitHandler>();
            return services;
        }
    }
}
