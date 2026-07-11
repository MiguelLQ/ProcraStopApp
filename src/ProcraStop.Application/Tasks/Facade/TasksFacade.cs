
using ProcraStop.Application.Tasks.Commands.ArchiveTask;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.Commands.CreateTask;
using ProcraStop.Application.Tasks.Commands.UpdateTask;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Queries.GetTaskById;
using ProcraStop.Application.Tasks.Queries.GetTasks;

namespace ProcraStop.Application.Tasks.Facade
{
    public sealed class TasksFacade : ITasksFacade
    {
        private readonly GetTasksQueryHandler _getTasksHandler;
        private readonly CreateTaskCommandHandler _createTaskHandler;
        private readonly CompleteTaskCommandHandler _completeTaskHandler;
        private readonly ArchiveTaskCommandHandler _archiveTaskHandler;
        private readonly GetTaskByIdQueryHandler _getTaskByIdHandler;
        private readonly UpdateTaskCommandHandler _updateTaskHandler;
        public TasksFacade(
            GetTasksQueryHandler getTasksHandler,
            CreateTaskCommandHandler createTaskHandler,
            CompleteTaskCommandHandler completeTaskHandler,
            ArchiveTaskCommandHandler archiveTaskHandler,
            GetTaskByIdQueryHandler getTaskByIdHandler,
            UpdateTaskCommandHandler updateTaskHandler)
        {
            _getTasksHandler = getTasksHandler;
            _createTaskHandler = createTaskHandler;
            _completeTaskHandler = completeTaskHandler;
            _archiveTaskHandler = archiveTaskHandler;
            _getTaskByIdHandler = getTaskByIdHandler;
            _updateTaskHandler = updateTaskHandler;
        }

        public Task ArchiveTaskAsync(Guid taskId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CompleteTaskAsync(Guid taskId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId, bool includeArchived = false)
        {
            throw new NotImplementedException();
        }

        public Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
           bool includeArchived = false,
           CancellationToken ct = default)
           => _getTasksHandler.HandleAsync(
               new GetTasksQuery { IncludeArchived = includeArchived },
               ct);

        public Task UpdateTaskAsync(Guid taskId, UpdateTaskCommand request, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
