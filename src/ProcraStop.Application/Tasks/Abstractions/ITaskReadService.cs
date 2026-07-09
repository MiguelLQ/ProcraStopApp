
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Analytics;

namespace ProcraStop.Application.Tasks.Abstractions
{
    public interface ITaskReadService
    {
        Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<TaskListItemDto>> GetListAsync(
                    bool includeArchived,
                    CancellationToken ct);
        Task<IReadOnlyList<TaskListItemDto>> SearchAsync(
                    string searchText,
                    bool includeArchived,
                    DateTime? dueDate,
                    bool withoutDueDate,
                    int maxResults,
                    CancellationToken ct); // editbycodex: lectura optimizada para el buscador.
        Task<IReadOnlyList<TaskDaySummaryDto>> GetDaySummariesAsync(
                    DateTime startDate,
                    DateTime endDate,
                    CancellationToken ct); // editbycodex: una sola lectura para los badges del rango visible.
        Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(
                    CancellationToken ct);
        Task<IReadOnlyList<TaskAnalyticsSourceItemDto>> GetAnalyticsDataAsync(
                    CancellationToken ct); // editbycodex: lectura minima y dedicada para TaskAnalyticsPage.
    }
}
