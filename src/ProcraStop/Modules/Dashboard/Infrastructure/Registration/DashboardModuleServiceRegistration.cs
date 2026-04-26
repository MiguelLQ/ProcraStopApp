

using ProcraStop.Modules.Dashboard.Application.Abstractions;
using ProcraStop.Modules.Dashboard.Application.Core;
using ProcraStop.Modules.Dashboard.Application.Queries;
using ProcraStop.Modules.Dashboard.Application.Services;
using ProcraStop.Modules.Dashboard.Infrastructure.Repositories;
using ProcraStop.Modules.Dashboard.Presentation.ViewModels;
using ProcraStop.Modules.Dashboard.Presentation.Views;

namespace ProcraStop.Modules.Dashboard.Infrastructure.Registration
{
    public static class DashboardModuleServiceRegistration
    {
        public static IServiceCollection AddDashboardModule(this IServiceCollection services)
        {
            //dashboard principal
            services.AddTransient<GetDashboardDecisionHandler>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DashboardView>();
            // infre
            // DASHBOARD
            services.AddScoped<IDashboardProjectionService, DashboardProjectionService>();
            services.AddScoped<GetDashboardDecisionHandler>();

            // SNAPSHOT
            services.AddScoped<ISystemSnapshotRepository, SystemSnapshotRepository>();

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
