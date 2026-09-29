using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.UnitTests;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public void Fresh_database_contains_all_current_modules_and_can_initialize_twice()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connection));
        using var provider = services.BuildServiceProvider();

        PersistenceDependencyInjection.InitializeDatabase(provider);
        PersistenceDependencyInjection.InitializeDatabase(provider);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        using var reader = command.ExecuteReader();
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
            tables.Add(reader.GetString(0));

        Assert.Contains("Tasks", tables);
        Assert.Contains("Subtasks", tables);
        Assert.Contains("Courses", tables);
        Assert.Contains("ClassSchedules", tables);
        Assert.Contains("Habits", tables);
        Assert.Contains("HabitLogs", tables);
        Assert.Contains("StressRecords", tables);
        Assert.Contains("StudyTechniqueSessions", tables);
        Assert.Contains("AcademicHelpRequests", tables);
        Assert.Contains("SystemSnapshots", tables);
    }
}
