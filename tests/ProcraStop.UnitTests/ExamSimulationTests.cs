using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Application.StudyTechniques.Services;
using ProcraStop.Domain.StudyTechniques.Entities;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.UnitTests;

public sealed class ExamSimulationTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Simulation_rejects_scores_outside_0_to_100(int score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExamSimulation.Create(Guid.NewGuid(), score, null));
    }

    [Fact]
    public void Simulation_normalizes_blank_errors()
    {
        var simulation = ExamSimulation.Create(Guid.NewGuid(), 80, "   ");

        Assert.Null(simulation.Errors);
        Assert.Equal(80, simulation.Score);
    }

    [Fact]
    public void Simulation_rejects_errors_longer_than_1000_characters()
    {
        var errors = new string('x', 1001);

        Assert.Throws<ArgumentException>(() =>
            ExamSimulation.Create(Guid.NewGuid(), 80, errors));
    }

    [Fact]
    public void Summary_tracks_attempts_last_best_and_improvement()
    {
        var examId = Guid.NewGuid();
        var simulations = new[]
        {
            Simulation(examId, 55, new DateTime(2026, 11, 1, 9, 0, 0), "Álgebra"),
            Simulation(examId, 70, new DateTime(2026, 11, 5, 9, 0, 0), null),
            Simulation(examId, 65, new DateTime(2026, 11, 7, 9, 0, 0), "Cálculo")
        };

        var summary = ExamSimulationService.BuildSummary(examId, simulations);

        Assert.True(summary.HasData);
        Assert.Equal(3, summary.Attempts);
        Assert.Equal(55, summary.FirstScore);
        Assert.Equal(65, summary.LastScore);
        Assert.Equal(70, summary.BestScore);
        Assert.Equal(10, summary.Improvement);
        Assert.Equal("Cálculo", summary.LastErrors);
        Assert.Equal(new DateTime(2026, 11, 7, 9, 0, 0), summary.LastTakenAtUtc);
    }

    [Fact]
    public void Empty_summary_has_no_data_and_no_improvement()
    {
        var summary = ExamSimulationService.BuildSummary(
            Guid.NewGuid(), Array.Empty<ExamSimulation>());

        Assert.False(summary.HasData);
        Assert.Null(summary.FirstScore);
        Assert.Null(summary.LastScore);
        Assert.Null(summary.BestScore);
        Assert.Null(summary.Improvement);
    }

    [Fact]
    public void Existing_database_gets_the_simulations_table_on_initialize()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connection));
        using var provider = services.BuildServiceProvider();

        // Simula una base anterior: se crea el esquema y luego se elimina la tabla nueva.
        PersistenceDependencyInjection.InitializeDatabase(provider);
        using (var drop = connection.CreateCommand())
        {
            drop.CommandText = "DROP TABLE \"ExamSimulations\";";
            drop.ExecuteNonQuery();
        }

        PersistenceDependencyInjection.InitializeDatabase(provider);

        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'ExamSimulations';";
        Assert.Equal(1L, Convert.ToInt64(command.ExecuteScalar()));
    }

    private static ExamSimulation Simulation(
        Guid examId,
        int score,
        DateTime takenAtUtc,
        string? errors) =>
        ExamSimulation.Rehydrate(
            Guid.NewGuid(),
            examId,
            score,
            errors,
            takenAtUtc);
}
