using ProcraStop.Application.StudyTechniques;
using ProcraStop.Infrastructure.StudyTechniques;

namespace ProcraStop.Modules.StudyTechniques;

public static class StudyTechniquesModule
{
    public static IServiceCollection AddStudyTechniquesModule(this IServiceCollection services)
    {
        services.AddStudyTechniquesApplication();
        services.AddStudyTechniquesInfrastructure();
        services.AddStudyTechniquesPresentation();

        return services;
    }
}
