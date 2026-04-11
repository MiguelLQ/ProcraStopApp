
namespace ProcraStop.Modules.Tasks.Application.Abstractions
{
    public interface ITaskReminderService
    {
        // Este método programa un recordatorio para una tarea específica.
        // Recibe el ID de la tarea, el título, una descripción opcional, la fecha del recordatorio, la fecha de vencimiento opcional y un token de cancelación.
        // El servicio se encargará de mostrar una notificación al usuario en la fecha del recordatorio.
        Task ScheduleAsync(
            Guid taskId,
            string title,
            string? description,
            DateTime reminderDate,
            DateTime? dueDate,
            CancellationToken cancellationToken = default);
        // Este método cancela un recordatorio previamente programado para una tarea específica.
        Task CancelAsync(
            Guid taskId,
            CancellationToken cancellationToken = default);
    }
}
