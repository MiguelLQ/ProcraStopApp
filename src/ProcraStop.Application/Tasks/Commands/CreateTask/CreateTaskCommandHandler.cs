

using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Entities;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ITaskReminderService _reminderService;

    public CreateTaskCommandHandler(
        ITaskRepository repository,
        ITaskReminderService reminderService)
    {
        _repository = repository;
        _reminderService = reminderService;
    }

    public async Task<Guid> HandleAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        // Calcular reminder (esto sí es responsabilidad del handler)
        DateTime? reminderDate = null;

        if (command.DueDate.HasValue && command.ReminderOffset != ReminderOffset.None)
        {
            reminderDate = command.DueDate.Value.AddMinutes(-(int)command.ReminderOffset);
        }

        // Crear entidad (dominio decide si es válido)
        var task = StudyTask.Create(
            command.Title,
            command.Description,
            command.DueDate,
            command.Priority,
            command.CourseId
        );

        // Aplicar reglas del dominio
        task.SetReminder(reminderDate);
        task.SetEstimatedMinutes(command.EstimatedMinutes);
        // Agregar subtareas (si las hay)
        if (command.Subtasks is not null)
        {
            foreach (var subtaskTitle in command.Subtasks)
            {
                if (!string.IsNullOrWhiteSpace(subtaskTitle))
                {
                    task.AddSubtask(subtaskTitle.Trim());
                }
            }
        }
        // Persistir
        await _repository.AddAsync(task, cancellationToken);

        // Integración externa (notificaciones)
        if (task.ReminderDate.HasValue && task.ReminderDate.Value > DateTime.Now)
        {
            await _reminderService.ScheduleAsync(
                task.Id,
                task.Title,
                task.Description,
                task.ReminderDate.Value,
                task.DueDate,
                cancellationToken);
        }


        return task.Id;
    }
}