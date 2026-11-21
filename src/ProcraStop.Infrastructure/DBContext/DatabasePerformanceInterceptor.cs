using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ProcraStop.Infrastructure.DBContext;

public sealed class DatabasePerformanceInterceptor : DbCommandInterceptor
{
    private const double SlowCommandThresholdMilliseconds = 100;
    private readonly ILogger<DatabasePerformanceInterceptor> _logger;

    public DatabasePerformanceInterceptor(ILogger<DatabasePerformanceInterceptor> logger)
    {
        _logger = logger;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogIfSlow(command, eventData.Duration, eventData.Context?.GetType().Name);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration, eventData.Context?.GetType().Name);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(
        DbCommand command, CommandExecutedEventData eventData, int result)
    {
        LogIfSlow(command, eventData.Duration, eventData.Context?.GetType().Name);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration, eventData.Context?.GetType().Name);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(
        DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        LogIfSlow(command, eventData.Duration, eventData.Context?.GetType().Name);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData.Duration, eventData.Context?.GetType().Name);
        return ValueTask.FromResult(result);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        => LogFailure(command, eventData.Duration, eventData.Context?.GetType().Name, eventData.Exception);

    public override Task CommandFailedAsync(
        DbCommand command, CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        LogFailure(command, eventData.Duration, eventData.Context?.GetType().Name, eventData.Exception);
        return Task.CompletedTask;
    }

    private void LogIfSlow(DbCommand command, TimeSpan elapsed, string? contextName)
    {
        if (elapsed.TotalMilliseconds < SlowCommandThresholdMilliseconds ||
            !_logger.IsEnabled(LogLevel.Warning))
            return;

        _logger.LogWarning(
            "[PERF] Database {CommandKind} on {ContextName} completed in {ElapsedMilliseconds:F1} ms",
            GetCommandKind(command), contextName ?? nameof(AppDbContext), elapsed.TotalMilliseconds);
    }

    private void LogFailure(
        DbCommand command, TimeSpan elapsed, string? contextName, Exception exception)
    {
        _logger.LogError(
            exception,
            "Database {CommandKind} on {ContextName} failed after {ElapsedMilliseconds:F1} ms",
            GetCommandKind(command), contextName ?? nameof(AppDbContext), elapsed.TotalMilliseconds);
    }

    private static string GetCommandKind(DbCommand command)
    {
        var text = command.CommandText.AsSpan().TrimStart();
        var length = 0;
        while (length < text.Length && !char.IsWhiteSpace(text[length]))
            length++;

        return length == 0 ? "SQL" : text[..length].ToString();
    }
}
