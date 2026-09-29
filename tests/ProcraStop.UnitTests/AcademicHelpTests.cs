using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Domain.StudyTechniques.Enums;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.UnitTests;

public sealed class AcademicHelpTests
{
    [Fact]
    public void Help_request_requires_a_question_and_follows_the_workflow()
    {
        Assert.Throws<ArgumentException>(() => AcademicHelpRequest.Create(
            null, null, "Álgebra", " ", HelpChannel.Classmate, "Ana"));

        var request = AcademicHelpRequest.Create(
            null, null, "Álgebra", "¿Cómo despejo x?", HelpChannel.Classmate, "Ana");
        Assert.Equal(HelpRequestStatus.Planned, request.Status);
        Assert.Throws<InvalidOperationException>(request.MarkResolved);

        request.MarkRequested();
        Assert.NotNull(request.RequestedAtUtc);
        request.MarkResolved();
        Assert.NotNull(request.ResolvedAtUtc);
        Assert.Equal(HelpRequestStatus.Resolved, request.Status);
    }

    [Fact]
    public async Task Help_request_can_be_saved_and_followed_up_in_sqlite()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connection));
        ProcraStop.Infrastructure.StudyTechniques.DependencyInjection
            .AddStudyTechniquesInfrastructure(services);
        ProcraStop.Application.StudyTechniques.DependencyInjection
            .AddStudyTechniquesApplication(services);
        using var provider = services.BuildServiceProvider();
        PersistenceDependencyInjection.InitializeDatabase(provider);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AcademicHelpService>();

        var id = await service.PlanAsync(null, null, "Cálculo", "¿Qué regla aplico?",
            HelpChannel.Teacher, "Docente");
        await service.MarkRequestedAsync(id);
        await service.MarkResolvedAsync(id);

        var saved = Assert.Single(await service.GetRecentAsync());
        Assert.Equal(id, saved.Id);
        Assert.Equal(HelpRequestStatus.Resolved, saved.Status);
        Assert.Equal("¿Qué regla aplico?", saved.Question);
    }
}
