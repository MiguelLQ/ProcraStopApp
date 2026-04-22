
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Modules.Stress.Application.Abstractions;
using ProcraStop.Modules.Stress.Application.Commands.RegisterStress;
using ProcraStop.Modules.Stress.Application.EngineStress;
using ProcraStop.Modules.Stress.Application.Queries;
using ProcraStop.Modules.Stress.Application.Services;
using ProcraStop.Modules.Stress.Infrastructure.Repositories;

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
            return services;
        }
    }
}
