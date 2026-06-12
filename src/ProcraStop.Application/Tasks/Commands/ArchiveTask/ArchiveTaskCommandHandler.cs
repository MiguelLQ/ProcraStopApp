

using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.ArchiveTask
{
    public sealed class ArchiveTaskCommandHandler
    {
        private readonly ITaskRepository _repository;

        public ArchiveTaskCommandHandler(ITaskRepository repository)
        {
            _repository = repository;

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
        }
    }
}
