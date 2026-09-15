using System.Diagnostics;
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Models;

namespace ProcraStop.Application.Dashboard.Services;

public sealed class DashboardProjectionCache :
    IDashboardProjectionCache,
    IDashboardCacheInvalidator,
    IDisposable
{
    private static readonly long CacheDurationTicks = Stopwatch.Frequency * 30L;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DashboardReadModel? _value;
    private long _expiresAtTimestamp;
    private long _generation;

    public async Task<DashboardReadModel> GetOrCreateAsync(
        Func<CancellationToken, Task<DashboardReadModel>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var cached = Volatile.Read(ref _value);
        if (IsValid(cached))
            return cached!;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            cached = Volatile.Read(ref _value);
            if (IsValid(cached))
                return cached!;

            var generation = Volatile.Read(ref _generation);
            var created = await factory(cancellationToken);

            if (generation == Volatile.Read(ref _generation))
            {
                Volatile.Write(ref _value, created);
                Volatile.Write(
                    ref _expiresAtTimestamp,
                    Stopwatch.GetTimestamp() + CacheDurationTicks);
            }

            return created;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Invalidate()
    {
        Interlocked.Increment(ref _generation);
        Volatile.Write(ref _value, null);
        Volatile.Write(ref _expiresAtTimestamp, 0);
    }

    private bool IsValid(DashboardReadModel? cached) =>
        cached is not null &&
        Stopwatch.GetTimestamp() < Volatile.Read(ref _expiresAtTimestamp);

    public void Dispose() => _gate.Dispose();
}
