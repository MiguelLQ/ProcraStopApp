using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using ProcraStop.Application.Tasks.Abstractions;

namespace ProcraStop.Infrastructure.Tasks.Services
{
    public class TaskReminderService : ITaskReminderService
    {
        public async Task ScheduleAsync(
            Guid taskId,
            string title,
            string? description,
            DateTime reminderDate,
            DateTime? dueDate,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"📅 ORIGINAL: {reminderDate}");

            // 🔥 FORZAR LOCAL CORRECTO
            var localReminder = new DateTime(
                reminderDate.Year,
                reminderDate.Month,
                reminderDate.Day,
                reminderDate.Hour,
                reminderDate.Minute,
                0,
                DateTimeKind.Local);

            Console.WriteLine($"✅ LOCAL FIX: {localReminder}");

            if (localReminder <= DateTime.Now.AddSeconds(10))
            {
                Console.WriteLine("❌ Muy cercano o pasado");
                return;
            }

            var notificationId = Math.Abs(taskId.GetHashCode());

            var request = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = $"⏰ {title}",
                Description = dueDate.HasValue
                    ? $"Vence: {dueDate:dd/MM HH:mm}"
                    : description ?? "Tarea pendiente",

                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = localReminder
                }
            };
            request.Android.ChannelId = "default_channel_id";
           

            Console.WriteLine("📢 Enviando a Android...");
            await Task.Delay(500);
            await LocalNotificationCenter.Current.Show(request);
        }

        public Task CancelAsync(
            Guid taskId,
            CancellationToken cancellationToken = default)
        {
            var notificationId = BuildNotificationId(taskId);

            LocalNotificationCenter.Current.Cancel(notificationId);

            return Task.CompletedTask;
        }

        //Genera un ID estable por tarea
        private static int BuildNotificationId(Guid taskId)
        {
            return Math.Abs(taskId.GetHashCode());
        }

        //Construcción del mensaje (UX importante)
        private static string BuildBody(string? description, DateTime? dueDate)
        {
            if (dueDate.HasValue)
            {
                return $"Vence: {dueDate.Value:dd/MM/yyyy hh:mm tt}";
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            return "Tienes una tarea pendiente.";
        }
    }
}
