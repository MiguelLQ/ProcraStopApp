
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.Dashboard.Core;
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Queries;
using ProcraStop.Application.Dashboard.Services;
using ProcraStop.Application.Decisions;

namespace ProcraStop.Application.Dashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDashboardApplication(
       this IServiceCollection services)
        {
            // core
            services.AddScoped<DecisionContextBuilder>();
            // servicios
            services.AddScoped<IDashboardProjectionService, DashboardProjectionService>();
            //CQRS
            services.AddTransient<GetDashboardDecisionHandler>();
            // CORE BUILDERS
            services.AddScoped<SystemStateBuilder>();
            services.AddScoped<CategoryBalanceBuilder>();
            services.AddScoped<TrendEngine>();
            // SIGNAL PROJECTION
            services.AddScoped<ISignalProjectionService, SignalProjectionService>();
            return services;
        }
    }
}
