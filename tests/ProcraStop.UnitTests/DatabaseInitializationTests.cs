using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcraStop.Infrastructure.DBContext;
using ProcraStop.Application.Notifications;
using ProcraStop.Infrastructure.Tasks.Repositories;

namespace ProcraStop.UnitTests;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public async Task Fresh_database_contains_all_current_modules_and_can_initialize_twice()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connection));
        using var provider = services.BuildServiceProvider();

        await PersistenceDependencyInjection.InitializeDatabaseAsync(provider);
        await PersistenceDependencyInjection.InitializeDatabaseAsync(provider);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        using var reader = command.ExecuteReader();
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
            tables.Add(reader.GetString(0));
        reader.Dispose();
        command.Dispose();

        Assert.Contains("Tasks", tables);
        Assert.Contains("Subtasks", tables);
        Assert.Contains("Courses", tables);
        Assert.Contains("ClassSchedules", tables);
        Assert.Contains("Habits", tables);
        Assert.Contains("HabitLogs", tables);
        Assert.Contains("StressRecords", tables);
        Assert.Contains("StudyTechniqueSessions", tables);
        Assert.Contains("ExamSimulations", tables);
        Assert.Contains("AcademicHelpRequests", tables);
        Assert.Contains("SystemSnapshots", tables);
        Assert.Contains("ReminderHistory", tables);
        Assert.Contains("ReminderSchedules", tables);
        Assert.Contains("__EFMigrationsHistory", tables);

        var history = new ReminderHistoryRepository(
            provider.GetRequiredService<IDbContextFactory<AppDbContext>>());
        var item = new ReminderHistoryItem(
            Guid.NewGuid(), Guid.NewGuid(), "DueReminder",
            ReminderEventStatus.ScheduledExact, "Prueba",
            DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, null);
        await history.AddAsync(item);

        var saved = Assert.Single(await history.GetRecentAsync());
        Assert.Equal(item.Id, saved.Id);
        Assert.Equal(ReminderEventStatus.ScheduledExact, saved.Status);
    }
}
