using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.StartTask;

public sealed class StartTaskCommandHandler
{
    private readonly ITaskRepository _repository;

    public StartTaskCommandHandler(ITaskRepository repository) => _repository = repository;

    public async Task HandleAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _repository.GetByIdAsync(taskId, ct)
            ?? throw new InvalidOperationException("Tarea no encontrada.");
        task.Start();
        await _repository.UpdateAsync(task, ct);
    }
}
