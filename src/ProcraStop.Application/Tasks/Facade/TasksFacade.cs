
using ProcraStop.Application.Tasks.Commands.ArchiveTask;
using ProcraStop.Application.Tasks.Commands.CompleteTask;
using ProcraStop.Application.Tasks.Commands.CreateTask;
using ProcraStop.Application.Tasks.Commands.UpdateTask;
using ProcraStop.Application.Tasks.DTOs;
using ProcraStop.Application.Tasks.Queries.GetTaskById;
using ProcraStop.Application.Tasks.Queries.GetTasks;
using ProcraStop.Application.Common;

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
            => _archiveTaskHandler.HandleAsync(
                new ArchiveTaskCommand { TaskId = taskId },
                ct);

        public Task CompleteTaskAsync(Guid taskId, CancellationToken ct = default)
            => _completeTaskHandler.HandleAsync(
                new CompleteTaskCommand { TaskId = taskId },
                ct);

        public async Task<OperationResult<Guid>> CreateTaskAsync(
            CreateTaskCommand command, CancellationToken ct = default)
        {
            try { return OperationResult<Guid>.Success(await _createTaskHandler.HandleAsync(command, ct)); }
            catch (Exception ex) { return OperationResultMapper.From<Guid>(ex); }
        }

        public Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId, bool includeArchived = false)
            => GetTaskByIdAsync(taskId, CancellationToken.None);

        public Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId, CancellationToken ct = default)
            => _getTaskByIdHandler.HandleAsync(
                new GetTaskByIdQuery { TaskId = taskId },
                ct);

        public Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
           bool includeArchived = false,
           CancellationToken ct = default)
           => _getTasksHandler.HandleAsync(
               new GetTasksQuery { IncludeArchived = includeArchived },
               ct);

        public async Task<OperationResult> UpdateTaskAsync(Guid taskId, UpdateTaskCommand request,
            CancellationToken ct = default)
        {
            if (request is null)
                return OperationResult.Failure(OperationError.ValidationError, "La solicitud es obligatoria.");

            try
            {
                await _updateTaskHandler.HandleAsync(new UpdateTaskCommand
                {
                    Id = taskId,
                    Title = request.Title,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    ReminderDate = request.ReminderDate,
                    ReminderOffset = request.ReminderOffset,
                    Priority = request.Priority,
                    CourseId = request.CourseId,
                    EstimatedMinutes = request.EstimatedMinutes,
                    Subtasks = request.Subtasks,
                    ActivityType = request.ActivityType,
                    PlannedStartDate = request.PlannedStartDate,
                    FirstStep = request.FirstStep,
                    PlannedReviewDate = request.PlannedReviewDate
                }, ct);
                return OperationResult.Success();
            }
            catch (Exception ex) { return OperationResultMapper.From(ex); }
        }
    }
}
