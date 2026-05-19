using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Abstractions
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
