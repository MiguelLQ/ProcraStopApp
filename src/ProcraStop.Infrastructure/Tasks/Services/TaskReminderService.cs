using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using ProcraStop.Application.Tasks.Abstractions;
using ProcraStop.Domain.Tasks.Enums;

namespace ProcraStop.Infrastructure.Tasks.Services
{
    public class TaskReminderService : ITaskReminderService
    {
        private enum ReminderKind
        {
            DueReminder = 0,
            StartReminder = 1
        }

        public Task ScheduleAsync(
            Guid taskId,
            string title,
            string? description,
            DateTime reminderDate,
            DateTime? dueDate,
            TaskPriority priority,
            CancellationToken cancellationToken = default)
            => ScheduleAsync(taskId, ReminderKind.DueReminder, title, description, reminderDate, dueDate, cancellationToken);

        public Task ScheduleStartReminderAsync(
            Guid taskId,
            string title,
            string? description,
            DateTime plannedStartDate,
            TaskPriority priority,
            CancellationToken cancellationToken = default)
            => ScheduleAsync(taskId, ReminderKind.StartReminder, title, description, plannedStartDate, null, cancellationToken);

        private static async Task ScheduleAsync(
            Guid taskId,
            ReminderKind kind,
            string title,
            string? description,
            DateTime reminderDate,
            DateTime? dueDate,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var notificationId = BuildNotificationId(taskId, kind);
            LocalNotificationCenter.Current.Cancel(notificationId);

            var localReminder = reminderDate.Kind == DateTimeKind.Utc
                ? reminderDate.ToLocalTime()
                : reminderDate;
            localReminder = new DateTime(
                localReminder.Year,
                localReminder.Month,
                localReminder.Day,
                localReminder.Hour,
                localReminder.Minute,
                0,
                DateTimeKind.Local);

            if (localReminder <= DateTime.Now.AddSeconds(10))
                return;

            var isStartReminder = kind == ReminderKind.StartReminder;
            var request = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = isStartReminder ? $"🚀 ¿Empezamos? {title}" : $"⏰ {title}",
                Description = isStartReminder
                    ? string.IsNullOrWhiteSpace(description)
                        ? "Llegó tu fecha de inicio. Da el primer paso."
                        : description
                    : BuildDueBody(description, dueDate),
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = localReminder
                }
            };
            request.Android.ChannelId = "default_channel_id";

            await LocalNotificationCenter.Current.Show(request);
        }

        public Task CancelDueReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LocalNotificationCenter.Current.Cancel(BuildNotificationId(taskId, ReminderKind.DueReminder));
            return Task.CompletedTask;
        }

        public Task CancelStartReminderAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LocalNotificationCenter.Current.Cancel(BuildNotificationId(taskId, ReminderKind.StartReminder));
            return Task.CompletedTask;
        }

        public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LocalNotificationCenter.Current.Cancel(BuildNotificationId(taskId, ReminderKind.DueReminder));
            LocalNotificationCenter.Current.Cancel(BuildNotificationId(taskId, ReminderKind.StartReminder));
            return Task.CompletedTask;
        }

        // IDs estables y sin colisión entre el aviso de vencimiento y el de inicio de la misma tarea.
        private static int BuildNotificationId(Guid taskId, ReminderKind kind)
        {
            var bytes = taskId.ToByteArray();
            var hash = BitConverter.ToUInt32(bytes, 0)
                       ^ BitConverter.ToUInt32(bytes, 4)
                       ^ BitConverter.ToUInt32(bytes, 8)
                       ^ BitConverter.ToUInt32(bytes, 12);
            var baseId = (int)(hash & 0x3FFFFFFF);
            return kind == ReminderKind.StartReminder ? (baseId << 1) | 1 : baseId << 1;
        }

        private static string BuildDueBody(string? description, DateTime? dueDate)
        {
            if (dueDate.HasValue)
                return $"Vence: {dueDate.Value:dd/MM/yyyy hh:mm tt}";

            if (!string.IsNullOrWhiteSpace(description))
                return description;

            return "Tienes una tarea pendiente.";
        }
    }
}
