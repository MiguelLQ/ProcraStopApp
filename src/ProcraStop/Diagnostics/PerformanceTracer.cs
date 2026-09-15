using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ProcraStop.Diagnostics;

public sealed class PerformanceTracer
{
    private readonly ILogger<PerformanceTracer> _logger;

    public PerformanceTracer(ILogger<PerformanceTracer> logger)
    {
        _logger = logger;
    }

    public long GetTimestamp() => Stopwatch.GetTimestamp();

    public IDisposable Measure(string category, string operation)
        => new Measurement(this, category, operation, GetTimestamp());

    public void LogElapsed(string category, string operation, long startedAt)
    {
        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        _logger.LogInformation(
            "[PERF] {Category} {Operation} completed in {ElapsedMilliseconds:F1} ms",
            category,
            operation,
            elapsed.TotalMilliseconds);
    }

    private sealed class Measurement : IDisposable
    {
        private readonly PerformanceTracer _owner;
        private readonly string _category;
        private readonly string _operation;
        private readonly long _startedAt;
        private bool _disposed;

        public Measurement(
            PerformanceTracer owner,
            string category,
            string operation,
            long startedAt)
        {
            _owner = owner;
            _category = category;
            _operation = operation;
            _startedAt = startedAt;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _owner.LogElapsed(_category, _operation, _startedAt);
        }
    }
}
