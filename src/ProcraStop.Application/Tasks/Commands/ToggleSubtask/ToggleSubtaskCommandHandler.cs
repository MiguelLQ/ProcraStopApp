
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Application.Tasks.Services;

namespace ProcraStop.Application.Tasks.Commands.ToggleSubtask
{
    public sealed class ToggleSubtaskCommandHandler
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskReminderService _reminderService;

        public ToggleSubtaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService)
        {
            _repository = repository;
            _reminderService = reminderService;
        }

        public async Task HandleAsync(
            ToggleSubtaskCommand command,
            CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(
                command.TaskId,
                ct);

            if (task is null)
                throw new Exception("Task no encontrada");

            // DOMAIN LOGIC
            task.ToggleSubtask(command.SubtaskId);

            // PERSIST
            await _repository.UpdateAsync(task, ct);
            await _reminderService.SyncTaskRemindersAsync(task, ct);
        }
    }
}
