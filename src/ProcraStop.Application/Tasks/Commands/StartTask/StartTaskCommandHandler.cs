using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Application.Tasks.Commands.StartTask;

public sealed class StartTaskCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ITaskReminderService _reminderService;

    public StartTaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService)
    {
        _repository = repository;
        _reminderService = reminderService;
    }

    public async Task HandleAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _repository.GetByIdAsync(taskId, ct)
            ?? throw new InvalidOperationException("Tarea no encontrada.");
        task.Start();
        await _repository.UpdateAsync(task, ct);

        // El primer avance ya ocurrió: el aviso de inicio deja de tener sentido.
        // El recordatorio de vencimiento se mantiene intacto.
        await _reminderService.CancelStartReminderAsync(task.Id, ct);
    }
}
