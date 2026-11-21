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
        private readonly TimeProvider _timeProvider;

        public UpdateTaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService,
            TimeProvider? timeProvider = null)
        {
            _repository = repository;
            _reminderService = reminderService;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }
        public async Task HandleAsync(UpdateTaskCommand command, CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(command.Id, ct);

            if (task is null)
                throw new KeyNotFoundException("Tarea no encontrada.");
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

            var reminderIntents = TaskReminderPlanBuilder.Build(task, _timeProvider);
            if (_repository is IReliableTaskRepository reliableRepository)
                await reliableRepository.UpdateWithReminderIntentsAsync(task, reminderIntents, ct);
            else
                await _repository.UpdateAsync(task, ct);
            await _reminderService.SyncTaskRemindersAsync(task, _timeProvider, ct);
        }
    }
}
