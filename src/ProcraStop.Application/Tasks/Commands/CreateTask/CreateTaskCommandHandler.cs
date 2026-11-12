

using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Entities;
using ProcraStop.Domain.Tasks.Enums;
using ProcraStop.Application.Tasks.Services;
using ProcraStop.Application.Tasks.DTOs;

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
        try
        {
            return await HandleAsyncCore(command, cancellationToken);
        }
        catch (Exception ex)
        {
            // Red de seguridad: nunca dejar caer la app al crear una tarea.
            throw new InvalidOperationException($"No se pudo crear la tarea: {ex.Message}", ex);
        }
    }

    private async Task<Guid> HandleAsyncCore(
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
        task.SetAcademicPlan(
            command.ActivityType,
            command.PlannedStartDate,
            command.FirstStep,
            command.PlannedReviewDate);
        IReadOnlyList<PlannedStepDto>? milestones = command.Milestones;
        if (command.ActivityType == AcademicActivityType.Exam)
        {
            if (!command.DueDate.HasValue)
                throw new InvalidOperationException("El examen necesita fecha límite.");
            milestones = ExamPlanBuilder.Build(
                command.Subtasks ?? [],
                command.PlannedStartDate ?? DateTime.Today,
                command.DueDate.Value);
        }
        if (milestones is not null)
        {
            foreach (var step in milestones.Where(x => !string.IsNullOrWhiteSpace(x.Title)))
                task.AddSubtask(step.Title.Trim(), plannedDate: step.PlannedDate);
        }
        // Agregar subtareas (si las hay)
        if (milestones is null && command.Subtasks is not null)
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

        // Integración externa: vencimiento + aviso de inicio previsto (EPA D1).
        try
        {
            await _reminderService.SyncTaskRemindersAsync(task, cancellationToken);
        }
        catch (Exception ex)
        {
            // No fallar la creación de tarea por un problema de recordatorios.
            System.Diagnostics.Debug.WriteLine($"[CreateTaskCommandHandler] Recordatorio falló: {ex}");
        }


        return task.Id;
    }
}
