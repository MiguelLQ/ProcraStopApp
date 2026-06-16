
using ProcraStop.Modules.Dashboard.Presentation.ViewModels;
using ProcraStop.Modules.Dashboard.Presentation.Views;

namespace ProcraStop.Modules.Dashboard
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDashboardPresentation(
            this IServiceCollection services)
        {
            //dashboard vista y viewmodels
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DashboardView>();
            return services;
        }
    }
}
