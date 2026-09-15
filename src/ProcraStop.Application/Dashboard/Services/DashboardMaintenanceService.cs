using ProcraStop.Application.Dashboard.Abstractions;
using ProcraStop.Application.Dashboard.Interfaces;
using ProcraStop.Application.Dashboard.Models;
using ProcraStop.Application.Stress.Services;

namespace ProcraStop.Application.Dashboard.Services;

public sealed class DashboardMaintenanceService : IDashboardMaintenanceService, IDisposable
{
    private readonly StressEventDetector _stressDetector;
    private readonly ISystemSnapshotRepository _historyRepository;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DashboardMaintenanceService(
        StressEventDetector stressDetector,
        ISystemSnapshotRepository historyRepository)
    {
        _stressDetector = stressDetector;
        _historyRepository = historyRepository;
    }

    public async Task RecordLatestStateAsync(
        DashboardReadModel dashboard,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dashboard);

        var taskSignals = dashboard.TaskSignals;
        var habitSignals = dashboard.HabitSignals;
        var snapshot = dashboard.CurrentSnapshot;
        if (taskSignals is null || habitSignals is null || snapshot is null)
            return;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await _stressDetector.EvaluateAndRegisterAsync(taskSignals, habitSignals);

            var history = await _historyRepository.GetLastDays(7);
            if (DashboardSnapshotPolicy.ShouldRecord(history.LastOrDefault(), snapshot))
                await _historyRepository.SaveAsync(snapshot);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}
