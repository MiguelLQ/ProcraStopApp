using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.StudyTechniques.Abstractions;
using ProcraStop.Infrastructure.StudyTechniques.Repositories;

namespace ProcraStop.Infrastructure.StudyTechniques;

public static class DependencyInjection
{
    public static IServiceCollection AddStudyTechniquesInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IStudyTechniqueSessionRepository, StudyTechniqueSessionRepository>();
        services.AddScoped<IAcademicHelpRepository, AcademicHelpRepository>();

        return services;
    }
}
