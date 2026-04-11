using ProcraStop.Modules.Tasks.Application.DTOs;

namespace ProcraStop.Modules.Tasks.Application.Facade
{
    public interface ITasksFacade
    {
        Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
            bool includeArchived = false,
            CancellationToken ct = default);

        Task<Guid> CreateTaskAsync(
            CreateTaskRequest request,
            CancellationToken ct = default);

        Task CompleteTaskAsync(
            Guid taskId,
            CancellationToken ct = default);

        Task ArchiveTaskAsync(
            Guid taskId,
            CancellationToken ct = default);
    }
}
