
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Core.Decisions.Context;
using ProcraStop.Core.Decisions.Engine;
using ProcraStop.Core.Decisions.Memory;
using ProcraStop.Core.Decisions.Rules;
using ProcraStop.Core.Decisions.Rules.Behavior;

namespace ProcraStop.Core.Decisions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreDependencyInjection(
        this IServiceCollection services)
        {
            // core
            services.AddScoped<DecisionContextBuilder>();
            // Registrar store de decisiones en memoria y el motor de decisiones
            services.AddSingleton<IDecisionMemoryStore, InMemoryDecisionMemoryStore>();
            services.AddSingleton<DecisionEngine>();
            // core rules
            services.AddTransient<IDecisionRule, FakeProductivityRule>();
            services.AddTransient<IDecisionRule, BurnoutRiskRule>();
            services.AddTransient<IDecisionRule, FakeProductivityWithStressRule>();
            services.AddTransient<IDecisionRule, BurnoutWithStressRule>();
            services.AddTransient<IDecisionRule, LowStressHighEfficiencyRule>();
            services.AddTransient<IDecisionRule, OverloadWithStressRule>();
            services.AddTransient<IDecisionRule, StablePerformanceRule>();
            return services;
        }
    }
}
