using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.StudyTechniques.Commands.RegisterStudySession;
using ProcraStop.Application.StudyTechniques.Services;

namespace ProcraStop.Application.StudyTechniques;

public static class DependencyInjection
{
    public static IServiceCollection AddStudyTechniquesApplication(this IServiceCollection services)
    {
        services.AddScoped<StudyTechniqueCatalogService>();
        services.AddScoped<StudyRecommendationEngine>();
        services.AddScoped<StudyDashboardService>();
        services.AddScoped<RegisterStudySessionHandler>();
        services.AddScoped<AcademicHelpService>();

        return services;
    }
}
