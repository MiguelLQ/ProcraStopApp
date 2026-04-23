
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Application.Commands.RegisterStress;
using ProcraStop.Modules.Stress.Application.EngineStress;
using ProcraStop.Modules.Stress.Application.Queries;
using ProcraStop.Modules.Stress.Application.Services;
using ProcraStop.Modules.Stress.Infrastructure.Repositories;
using ProcraStop.Modules.Stress.Presentation.ViewModel;
using ProcraStop.Modules.Stress.Presentation.Views;

namespace ProcraStop.Modules.Stress.Infrastructure.Registration
{
    public static class StressModuleServiceRegistration
    {
        public static IServiceCollection AddStressModule(this IServiceCollection services)
        {
            // =========================
            // REPOSITORY
            // =========================
            services.AddScoped<IStressRepository, StressRepository>();

            // =========================
            // SERVICES (cálculo → signals)
            // =========================
            services.AddScoped<StressMetricsService>();
            services.AddScoped<StressEventDetector>();
            // =========================
            // CQRS (Application)
            // =========================
            services.AddScoped<RegisterStressHandler>();
            services.AddScoped<GetStressHistoryHandler>();

            // =========================
            // RULES (módulo Stress)
            // =========================
            services.AddScoped<IDecisionRule, HighStressRule>();
            services.AddScoped<IDecisionRule, RecoveryNeededRule>();
            services.AddScoped<IDecisionRule, OverloadWithStressRule>();
            services.AddScoped<IDecisionRule, HighStressRisingRule>();
            services.AddScoped<IDecisionRule, HighStressLowRecoveryRule>();
            services.AddScoped<IDecisionRule, RecoveryOpportunityRule>();
            // =========================
            // Views y ViewModels
            // =========================
            services.AddTransient<StressCheckInView>();
            services.AddTransient<StressCheckInViewModel>();
            return services;
        }
    }
}
