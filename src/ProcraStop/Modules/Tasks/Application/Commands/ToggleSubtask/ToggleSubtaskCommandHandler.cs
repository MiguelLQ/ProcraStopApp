using ProcraStop.Modules.Tasks.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcraStop.Modules.Tasks.Application.Commands.ToggleSubtask
{
    public sealed class ToggleSubtaskCommandHandler
    {
        private readonly ITaskRepository _repository;

        public ToggleSubtaskCommandHandler(ITaskRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(ToggleSubtaskCommand command, CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(command.TaskId, ct);

            if (task is null)
                throw new Exception("Task no encontrada");

            // DOMINIO HACE TODO
            task.ToggleSubtask(command.SubtaskId);

            await _repository.UpdateAsync(task, ct);
        }
    }
}
