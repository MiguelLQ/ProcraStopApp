using ProcraStop.Modules.Tasks.Application.Abstractions;
using ProcraStop.Modules.Tasks.Domain.Entities;
using ProcraStop.Modules.Tasks.Domain.Enums;

namespace ProcraStop.Modules.Tasks.Application.Commands.CreateTask;

public sealed class CreateTaskCommandHandler
{
    private readonly ITaskRepository _repository;
    private readonly ITaskReminderService _reminderService;

    public CreateTaskCommandHandler(ITaskRepository repository, ITaskReminderService reminderService)
    {
        _repository = repository;
        _reminderService = reminderService;
    }

    public async Task<Guid> HandleAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validamos que el título no esté vacío o solo contenga espacios en blanco
        if (string.IsNullOrWhiteSpace(command.Title))
            throw new InvalidOperationException("El título es obligatorio.");
        // Validamos que el tiempo estimado no sea negativo
        if (command.EstimatedMinutes is < 0)
            throw new InvalidOperationException("El tiempo estimado no puede ser negativo.");
        // Validamos que la fecha de recordatorio no sea posterior a la fecha límite
        if (command.ReminderDate.HasValue && command.DueDate.HasValue &&
            command.ReminderDate > command.DueDate)
            throw new InvalidOperationException("El recordatorio no puede ser posterior a la fecha límite.");
        // Calculamos la fecha de recordatorio basada en la fecha límite y el offset proporcionado
        DateTime? reminderDate = null;

        if (command.DueDate.HasValue && command.ReminderOffset != ReminderOffset.None)
        {
            reminderDate = command.DueDate.Value.AddMinutes(-(int)command.ReminderOffset);
        }
        // Validamos que la fecha de recordatorio calculada no sea pasada
        if (reminderDate.HasValue && reminderDate.Value <= DateTime.Now)
        {
            throw new InvalidOperationException("El recordatorio calculado ya pasó.");
        }
        Console.WriteLine($"OFFSET: {command.ReminderOffset}");
        Console.WriteLine($"DUEDATE: {command.DueDate}");
        // Creamos la tarea con los datos proporcionados, asegurándonos de limpiar los espacios en blanco
        var task = new StudyTask
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description)
                ? null
                : command.Description.Trim(),
            DueDate = command.DueDate,
            ReminderDate = reminderDate,
            Priority = command.Priority,
            Status = TaskItemStatus.Pending,
            IsArchived = false,
            CourseId = command.CourseId,
            EstimatedMinutes = command.EstimatedMinutes,
            CreatedAtUtc = DateTime.Now,
            CompletedAtUtc = null
        };
        // Guardamos la tarea antes de programar el recordatorio para asegurarnos de que el ID esté disponible
        await _repository.AddAsync(task, cancellationToken);
        // Solo programamos el recordatorio si la fecha de recordatorio es futura
        if (task.ReminderDate.HasValue && task.ReminderDate.Value > DateTime.Now)
        {
            Console.WriteLine("Voy a programar notificación...");
            await _reminderService.ScheduleAsync(
                task.Id,
                task.Title,
                task.Description,
                task.ReminderDate.Value,
                task.DueDate,
                cancellationToken);
            Console.WriteLine(" Notificación enviada al servicio");

        }
       
        Console.WriteLine($"NOW: {DateTime.Now}");
        Console.WriteLine($"REMINDER: {reminderDate}");
        // Si la fecha de recordatorio es pasada, no programamos el recordatorio pero permitimos crear la tarea
        return task.Id;
    }
}