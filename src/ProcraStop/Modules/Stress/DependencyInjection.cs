
using ProcraStop.Modules.Stress.Presentation.ViewModel;
using ProcraStop.Modules.Stress.Presentation.Views;

namespace ProcraStop.Modules.Stress
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddStressPresentation(
             this IServiceCollection services)
        {
            // =========================
            // Views y ViewModels
            // =========================
            services.AddTransient<StressCheckInView>();
            services.AddTransient<StressCheckInViewModel>();
            services.AddTransient<StressAnalyticsViewModel>();
            services.AddTransient<StressAnalyticsPage>();
            return services;
        }
    }
}
