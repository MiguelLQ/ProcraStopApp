
using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.ToggleSubtask
{
    public sealed class ToggleSubtaskCommandHandler
    {
        private readonly ITaskRepository _repository;

        public ToggleSubtaskCommandHandler(ITaskRepository repository)
        {
            _repository = repository;
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
        }
    }
}