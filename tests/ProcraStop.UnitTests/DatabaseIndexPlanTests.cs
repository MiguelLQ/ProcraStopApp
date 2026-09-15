using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProcraStop.Infrastructure.DBContext;

namespace ProcraStop.UnitTests;

public sealed class DatabaseIndexPlanTests
{
    [Theory]
    [InlineData(
        "SELECT Id FROM Tasks WHERE IsArchived = 0 AND DueDate >= '2026-01-01' AND DueDate < '2026-02-01'",
        "IX_Tasks_IsArchived_DueDate")]
    [InlineData(
        "SELECT Id FROM Tasks WHERE IsArchived = 0 ORDER BY CreatedAtUtc DESC",
        "IX_Tasks_IsArchived_CreatedAtUtc")]
    [InlineData(
        "SELECT Id FROM Habits WHERE IsActive = 1 ORDER BY Name",
        "IX_Habits_IsActive_Name")]
    [InlineData(
        "SELECT Id FROM HabitLogs WHERE Date >= '2026-01-01' AND Date < '2026-02-01'",
        "IX_HabitLogs_Date")]
    [InlineData(
        "SELECT Id FROM HabitLogs WHERE HabitId = 'habit' AND Date >= '2026-01-01' AND Date < '2026-02-01'",
        "IX_HabitLogs_HabitId_Date")]
    [InlineData(
        "SELECT Id FROM StressRecords WHERE Timestamp >= '2026-01-01' ORDER BY Timestamp DESC",
        "IX_StressRecords_Timestamp")]
    [InlineData(
        "SELECT Id FROM StressRecords WHERE Source = 1 AND Timestamp >= '2026-01-01'",
        "IX_StressRecords_Source_Timestamp")]
    [InlineData(
        "SELECT Id FROM SystemSnapshots WHERE Timestamp >= '2026-01-01' ORDER BY Timestamp",
        "IX_SystemSnapshots_Timestamp")]
    [InlineData(
        "SELECT Id FROM ClassSchedules WHERE IsActive = 1 AND DayOfWeek = 1 ORDER BY StartTicks",
        "IX_ClassSchedules_IsActive_DayOfWeek_StartTicks")]
    public void Frequent_query_uses_expected_index(string sql, string indexName)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        using var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        using var command = connection.CreateCommand();
        command.CommandText = $"EXPLAIN QUERY PLAN {sql}";
        using var reader = command.ExecuteReader();

        var details = new List<string>();
        while (reader.Read())
            details.Add(reader.GetString(3));

        Assert.Contains(details, detail =>
            detail.Contains(indexName, StringComparison.OrdinalIgnoreCase));
    }
}
