
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Abstractions
{
    public interface ITaskReadService
    {
        Task<TaskDetailDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<TaskListItemDto>> GetListAsync(
                    bool includeArchived,
                    CancellationToken ct);
        Task<IReadOnlyList<TaskDashboardDto>> GetDashboardDataAsync(
                    CancellationToken ct);
    }
}
