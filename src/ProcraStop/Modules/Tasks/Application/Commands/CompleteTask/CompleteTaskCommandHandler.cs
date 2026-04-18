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
                throw new Exception("Task no encontrada");

            // REGLA DE NEGOCIO (IMPORTANTE)
            if (task.Subtasks.Any())
                throw new InvalidOperationException("No puedes completar una tarea con subtareas manualmente");

            //DOMINIO
            task.Complete();

            //PERSISTENCIA
            await _repository.UpdateAsync(task, cancellationToken);
        }
    }
}
