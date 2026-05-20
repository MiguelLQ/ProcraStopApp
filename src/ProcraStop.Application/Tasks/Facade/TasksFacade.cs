
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Queries.GetTasks;

namespace ProcraStop.Application.Tasks.Facade
{
    public sealed class TasksFacade : ITasksFacade
    {
        private readonly GetTasksQueryHandler _getTasksHandler;
        public TasksFacade(GetTasksQueryHandler getTasksHandler)
        {
            _getTasksHandler = getTasksHandler;
        }

        public Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
           bool includeArchived = false,
           CancellationToken ct = default)
           => _getTasksHandler.HandleAsync(
               new GetTasksQuery { IncludeArchived = includeArchived },
               ct);
    }
}
