using ProcraStop.Application.Tasks.Abstractions;


namespace ProcraStop.Application.Tasks.Commands.UpdateTask
{
    public sealed class UpdateTaskCommandHandler
    {
        private readonly ITaskRepository _repository;

        public UpdateTaskCommandHandler(ITaskRepository repository)
        {
            _repository = repository;
        }
        public async Task HandleAsync(UpdateTaskCommand command, CancellationToken ct)
        {
            var task = await _repository.GetByIdAsync(command.Id, ct);

            if (task is null)
                throw new Exception("Task no encontrada");

            // dominio
            task.Update(
                command.Title,
                command.Description,
                command.DueDate,
                command.ReminderDate,
                command.Priority,
                command.CourseId,
                command.EstimatedMinutes,
                command.Subtasks
            );

            await _repository.UpdateAsync(task, ct);
        }
    }
}
