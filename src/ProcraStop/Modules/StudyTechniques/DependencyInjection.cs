using ProcraStop.Modules.StudyTechniques.Presentation.ViewModels;
using ProcraStop.Modules.StudyTechniques.Presentation.Views;

namespace ProcraStop.Modules.StudyTechniques;

public static class DependencyInjection
{
    public static IServiceCollection AddStudyTechniquesPresentation(this IServiceCollection services)
    {
        services.AddTransient<StudyTechniquesHomeViewModel>();
        services.AddTransient<StudyTechniquesPage>();
        services.AddTransient<StudyTechniquesViewModel>();
        services.AddTransient<TechniqueRecommendationPage>();
        services.AddTransient<TechniqueLibraryViewModel>();
        services.AddTransient<TechniqueLibraryPage>();
        services.AddTransient<StudySessionHistoryViewModel>();
        services.AddTransient<StudySessionHistoryPage>();
        services.AddTransient<PomodoroViewModel>();
        services.AddTransient<PomodoroPage>();
        services.AddTransient<AcademicHelpViewModel>();
        services.AddTransient<AcademicHelpPage>();

        return services;
    }
}
