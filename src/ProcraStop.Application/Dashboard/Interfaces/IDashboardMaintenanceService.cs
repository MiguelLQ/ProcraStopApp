using ProcraStop.Application.Dashboard.Models;

namespace ProcraStop.Application.Dashboard.Interfaces;

public interface IDashboardMaintenanceService
{
    Task RecordLatestStateAsync(
        DashboardReadModel dashboard,
        CancellationToken cancellationToken = default);
}
