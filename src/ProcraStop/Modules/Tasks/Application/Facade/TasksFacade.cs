using ProcraStop.Modules.Tasks.Application.Commands.ArchiveTask;
using ProcraStop.Modules.Tasks.Application.Commands.CompleteTask;
using ProcraStop.Modules.Tasks.Application.Commands.CreateTask;
using ProcraStop.Modules.Tasks.Application.DTOs;
using ProcraStop.Modules.Tasks.Application.Queries.GetCourses;
using ProcraStop.Modules.Tasks.Application.Queries.GetTaskById;
using ProcraStop.Modules.Tasks.Application.Queries.GetTasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.Facade
{
    public sealed class TasksFacade : ITasksFacade
    {
        private readonly GetTasksQueryHandler _getTasksHandler;
        private readonly CreateTaskCommandHandler _createTaskHandler;
        private readonly CompleteTaskCommandHandler _completeTaskHandler;
        private readonly ArchiveTaskCommandHandler _archiveTaskHandler;
        private readonly GetCoursesQueryHandler _getCoursesHandler;
        private readonly GetTaskByIdQueryHandler _getTaskByIdHandler;
        public TasksFacade(
            GetTasksQueryHandler getTasksHandler,
            CreateTaskCommandHandler createTaskHandler,
            CompleteTaskCommandHandler completeTaskHandler,
            ArchiveTaskCommandHandler archiveTaskHandler,
             GetCoursesQueryHandler getCoursesHandler,
             GetTaskByIdQueryHandler getTaskByIdHandler)
        {
            _getTasksHandler = getTasksHandler;
            _createTaskHandler = createTaskHandler;
            _completeTaskHandler = completeTaskHandler;
            _archiveTaskHandler = archiveTaskHandler;
            _getCoursesHandler = getCoursesHandler;
            _getTaskByIdHandler = getTaskByIdHandler;
        }

        public Task<IReadOnlyList<TaskListItemDto>> GetTasksAsync(
            bool includeArchived = false,
            CancellationToken ct = default)
            => _getTasksHandler.HandleAsync(
                new GetTasksQuery { IncludeArchived = includeArchived },
                ct);

        public Task<Guid> CreateTaskAsync(
            CreateTaskRequest request,
            CancellationToken ct = default)
        {
            var command = new CreateTaskCommand
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                ReminderDate = request.ReminderDate,
                ReminderOffset = request.ReminderOffset,
                Priority = request.Priority,
                CourseId = request.CourseId,
                EstimatedMinutes = request.EstimatedMinutes,
                Subtasks = request.Subtasks
            };

            return _createTaskHandler.HandleAsync(command, ct);
        }
        public Task CompleteTaskAsync(
            Guid taskId,
            CancellationToken ct = default)
            => _completeTaskHandler.HandleAsync(
                new CompleteTaskCommand { TaskId = taskId },
                ct);

        public Task ArchiveTaskAsync(
            Guid taskId,
            CancellationToken ct = default)
            => _archiveTaskHandler.HandleAsync(
                new ArchiveTaskCommand { TaskId = taskId },
                ct);
        public Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
            CancellationToken ct = default)
            => _getCoursesHandler.HandleAsync(new GetCoursesQuery(), ct);
        public Task<TaskDetailDto?> GetTaskByIdAsync(Guid taskId, CancellationToken ct = default)
            => _getTaskByIdHandler.HandleAsync(
                new GetTaskByIdQuery { TaskId = taskId },
                ct);
    }
}
