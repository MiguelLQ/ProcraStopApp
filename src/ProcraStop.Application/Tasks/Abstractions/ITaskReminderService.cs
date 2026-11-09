using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Application.Tasks.Abstractions
{
    public interface ITaskReminderService
    {
        // Programa el recordatorio de vencimiento de una tarea.
        Task ScheduleAsync(
            Guid taskId,
            string title,
            string? description,
            DateTime reminderDate,
            DateTime? dueDate,
            TaskPriority priority,
            CancellationToken cancellationToken = default);

        // Programa el aviso de inicio previsto. Es independiente del recordatorio de vencimiento,
        // de modo que una tarea puede tener ambos avisos activos a la vez.
        Task ScheduleStartReminderAsync(
            Guid taskId,
            string title,
            string? description,
            DateTime plannedStartDate,
            TaskPriority priority,
            CancellationToken cancellationToken = default);

        // Cancela solo el recordatorio de vencimiento.
        Task CancelDueReminderAsync(
            Guid taskId,
            CancellationToken cancellationToken = default);

        // Cancela solo el aviso de inicio previsto.
        Task CancelStartReminderAsync(
            Guid taskId,
            CancellationToken cancellationToken = default);

        // Cancela todos los avisos de la tarea (vencimiento e inicio).
        Task CancelAsync(
            Guid taskId,
            CancellationToken cancellationToken = default);
    }
}
