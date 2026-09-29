
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Commands.CompleteTask
{
    public sealed class CompleteTaskCommandHandler
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskReminderService _reminderService;

        public CompleteTaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService)
        {
            _repository = repository;
            _reminderService = reminderService;
        }

        public async Task HandleAsync(
            CompleteTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task is null)
                throw new Exception("Task no encontrada");

            // REGLA DE NEGOCIO (IMPORTANTE)
            if (task.Subtasks.Any() && task.ActivityType != AcademicActivityType.Deliverable)
                throw new InvalidOperationException("No puedes completar una tarea con subtareas manualmente");
            if (task.Subtasks.Any(s => !s.IsCompleted))
                throw new InvalidOperationException("Completa los hitos antes de entregar.");

            //DOMINIO
            task.Complete();

            //PERSISTENCIA
            await _repository.UpdateAsync(task, cancellationToken);
            await _reminderService.CancelAsync(task.Id, cancellationToken);
        }
    }
}
