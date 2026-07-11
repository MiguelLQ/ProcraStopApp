using ProcraStop.Modules.Academic.Presentation.ViewModels;
using ProcraStop.Modules.Academic.Presentation.Views;

namespace ProcraStop.Modules.Academic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAcademicPresentation(
            this IServiceCollection services)
        {
            services.AddTransient<CoursesPageViewModel>();
            services.AddTransient<CoursesPage>();
            services.AddTransient<SchedulePageViewModel>();
            services.AddTransient<SchedulePage>();

            return services;
        }
    }
}
