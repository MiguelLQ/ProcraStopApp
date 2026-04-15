using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Entities;

namespace ProcraStop.Modules.Tasks.Application.Commands.ArchiveTask
{
    public sealed class ArchiveTaskCommandHandler
    {
        private readonly ITaskRepository _repository;

        public ArchiveTaskCommandHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(
            ArchiveTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            /*ar task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);
*/
            //if (task is null)
                return;

            //var updated = new StudyTask
            //{
            //    Id = task.Id,
            //    Title = task.Title,
            //    Description = task.Description,
            //    DueDate = task.DueDate,
            //    ReminderDate = task.ReminderDate,
            //    Priority = task.Priority,
            //    Status = task.Status,
            //    IsArchived = true,
            //    CourseId = task.CourseId,
            //    EstimatedMinutes = task.EstimatedMinutes,
            //    CreatedAtUtc = task.CreatedAtUtc,
            //    CompletedAtUtc = task.CompletedAtUtc
            //};

            //await _repository.UpdateAsync(updated, cancellationToken);
        }
    }
}
