

using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.ArchiveTask
{
    public sealed class ArchiveTaskCommandHandler
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskReminderService _reminderService;

        public ArchiveTaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService)
        {
            _repository = repository;
            _reminderService = reminderService;

        }

        public async Task HandleAsync(
            ArchiveTaskCommand command
            , CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(command.TaskId, ct);

            if (task is null)
                throw new Exception("Task no encontrada");

            // dominio
            task.Archive();

            //persistencia
            await _repository.UpdateAsync(task, ct);
            await _reminderService.CancelAsync(task.Id, ct);
        }
    }
}
