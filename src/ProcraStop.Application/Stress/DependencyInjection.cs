
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Stress.Commands.RegisterStress;
using ProcraStop.Application.Stress.EngineStress.Rules;
using ProcraStop.Application.Stress.Queries.GetStressAnalytics;
using ProcraStop.Application.Stress.Queries.GetStressHistory;
using ProcraStop.Application.Stress.Services;
using ProcraStop.Core.Decisions.Rules;

namespace ProcraStop.Application.Stress
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStressApplication(
        this IServiceCollection services)
        {
            // =========================
            // SERVICES (cálculo → signals)
            // =========================
            services.AddScoped<StressMetricsService>();
            services.AddScoped<StressAnalyticsService>();
            services.AddScoped<StressEventDetector>();

            // =========================
            // CQRS (Application)
            // =========================
            services.AddScoped<RegisterStressHandler>();
            services.AddScoped<GetStressHistoryHandler>();
            services.AddScoped<GetStressAnalyticsHandler>();

            // =========================
            // RULES (módulo Stress)
            // =========================
            services.AddScoped<IDecisionRule, HighStressRule>();
            services.AddScoped<IDecisionRule, RecoveryNeededRule>();
            services.AddScoped<IDecisionRule, HighStressRisingRule>();
            services.AddScoped<IDecisionRule, HighStressLowRecoveryRule>();
            services.AddScoped<IDecisionRule, RecoveryOpportunityRule>();
            return services;
        }
    }
}
