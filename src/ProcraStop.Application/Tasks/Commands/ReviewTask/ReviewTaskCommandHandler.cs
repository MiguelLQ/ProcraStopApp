using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.ReviewTask;

public sealed class ReviewTaskCommandHandler
{
    private readonly ITaskRepository _repository;

    public ReviewTaskCommandHandler(ITaskRepository repository) => _repository = repository;

    public async Task HandleAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _repository.GetByIdAsync(taskId, ct)
            ?? throw new InvalidOperationException("Entregable no encontrado.");
        task.MarkReviewed();
        await _repository.UpdateAsync(task, ct);
    }
}
