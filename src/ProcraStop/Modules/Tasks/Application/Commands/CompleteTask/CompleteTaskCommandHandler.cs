using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.Commands.CompleteTask
{
    public sealed class CompleteTaskCommandHandler
    {
        private readonly ITaskRepository _repository;

        public CompleteTaskCommandHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            CompleteTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task is null)
                return;

            //var updated = new StudyTask
            //{
            //    Id = task.Id,
            //    Title = task.Title,
            //    Description = task.Description,
            //    DueDate = task.DueDate,
            //    ReminderDate = task.ReminderDate,
            //    Priority = task.Priority,
            //    Status = TaskItemStatus.Completed,
            //    IsArchived = task.IsArchived,
            //    CourseId = task.CourseId,
            //    EstimatedMinutes = task.EstimatedMinutes,
            //    CreatedAtUtc = task.CreatedAtUtc,
            //    CompletedAtUtc = DateTime.UtcNow
            //};

            //await _repository.UpdateAsync(updated, cancellationToken);
        }   
    }
}
