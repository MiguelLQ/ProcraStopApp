using ProcraStop.Application.Dashboard;
using ProcraStop.Infrastructure.Dashboard;

namespace ProcraStop.Modules.Dashboard
{
    public static class DashboardModule
    {
        public static IServiceCollection AddDashboardModule(
        this IServiceCollection services)
        {
            services.AddDashboardApplication();

            services.AddDashboardInfrastructure();

            services.AddDashboardPresentation();

            return services;
        }
    }
}
