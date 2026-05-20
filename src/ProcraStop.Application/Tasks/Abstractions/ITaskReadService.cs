
using ProcraStop.Application.Tasks.DTOs;

namespace ProcraStop.Application.Tasks.Abstractions
{
    public interface ITaskReadService
    {
        Task<IReadOnlyList<TaskListItemDto>> GetListAsync(
                    bool includeArchived,
                    CancellationToken ct);
    }
}
