using ProcraStop.Modules.Habits.Presentation.ViewModels;
using ProcraStop.Modules.Habits.Presentation.Views;

namespace ProcraStop.Modules.Habits
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHabitsPresentation(
             this IServiceCollection services)
        {
            // views y view models
            services.AddTransient<CreateHabitViewModel>();
            services.AddTransient<CreateHabitPage>();
            services.AddTransient<HabitListViewModel>();
            services.AddTransient<HabitListPage>();
            services.AddTransient<TrackHabitViewModel>();
            services.AddTransient<TrackHabitPage>();
            return services;
        }
    }
}
