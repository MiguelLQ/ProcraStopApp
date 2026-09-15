using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ProcraStop.Infrastructure.DBContext;

public sealed class DatabasePerformanceInterceptor : DbCommandInterceptor
{
    private const double SlowCommandThresholdMilliseconds = 100;
    private readonly ConcurrentDictionary<Guid, long> _startedCommands = new();
    private readonly ILogger<DatabasePerformanceInterceptor> _logger;

    public DatabasePerformanceInterceptor(ILogger<DatabasePerformanceInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Start(eventData.CommandId);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Start(eventData.CommandId);
        return ValueTask.FromResult(result);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        Start(eventData.CommandId);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Start(eventData.CommandId);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        Start(eventData.CommandId);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Start(eventData.CommandId);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name);
        return ValueTask.FromResult(result);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name, failed: true);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name, failed: true);
        return Task.CompletedTask;
    }

    public override void CommandCanceled(DbCommand command, CommandEndEventData eventData)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name, failed: true);
    }

    public override Task CommandCanceledAsync(
        DbCommand command,
        CommandEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Complete(command, eventData.CommandId, eventData.Context?.GetType().Name, failed: true);
        return Task.CompletedTask;
    }

    private void Start(Guid commandId)
        => _startedCommands[commandId] = Stopwatch.GetTimestamp();

    private void Complete(
        DbCommand command,
        Guid commandId,
        string? contextName,
        bool failed = false)
    {
        if (!_startedCommands.TryRemove(commandId, out var startedAt))
            return;

        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        var commandKind = command.CommandText
            .TrimStart()
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "SQL";

        if (failed || elapsed.TotalMilliseconds >= SlowCommandThresholdMilliseconds)
        {
            _logger.LogWarning(
                "[PERF] Database {CommandKind} on {ContextName} completed in {ElapsedMilliseconds:F1} ms. Failed={Failed}",
                commandKind,
                contextName ?? nameof(AppDbContext),
                elapsed.TotalMilliseconds,
                failed);
            return;
        }

        _logger.LogInformation(
            "[PERF] Database {CommandKind} on {ContextName} completed in {ElapsedMilliseconds:F1} ms",
            commandKind,
            contextName ?? nameof(AppDbContext),
            elapsed.TotalMilliseconds);
    }
}
