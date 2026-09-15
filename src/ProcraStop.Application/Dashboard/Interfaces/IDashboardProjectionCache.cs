using ProcraStop.Application.Dashboard.Models;

namespace ProcraStop.Application.Dashboard.Interfaces;

public interface IDashboardProjectionCache
{
    Task<DashboardReadModel> GetOrCreateAsync(
        Func<CancellationToken, Task<DashboardReadModel>> factory,
        CancellationToken cancellationToken = default);
}

public interface IDashboardCacheInvalidator
{
    void Invalidate();
}
