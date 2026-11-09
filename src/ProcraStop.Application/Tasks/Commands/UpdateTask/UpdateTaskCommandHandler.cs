using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Mappers;
using ProcraStop.Application.Tasks.Services;
using ProcraStop.Domain.Tasks.Entities;
namespace ProcraStop.Application.Tasks.Commands.UpdateTask
{
    public sealed class UpdateTaskCommandHandler
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskReminderService _reminderService;

        public UpdateTaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService)
        {
            _repository = repository;
            _reminderService = reminderService;
        }
        public async Task HandleAsync(UpdateTaskCommand command, CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(command.Id, ct);

            if (task is null)
                throw new Exception("Task no encontrada");
            // mapeo de subtareas
            var subtasks = command.Subtasks?
            .Select(x => x.ToDomain())
            .ToList()
            ?? new List<Subtask>();
            // dominio
            task.Update(
                command.Title,
                command.Description,
                command.DueDate,
                command.ReminderDate,
                command.Priority,
                command.CourseId,
                command.EstimatedMinutes,
                subtasks,
                command.ActivityType,
                command.PlannedStartDate,
                command.FirstStep,
                command.PlannedReviewDate
            );

            await _repository.UpdateAsync(task, ct);
            await _reminderService.SyncTaskRemindersAsync(task, ct);
        }
    }
}
