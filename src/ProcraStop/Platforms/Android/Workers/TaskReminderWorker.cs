using Android.Content;
using AndroidX.Work;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace ProcraStop.Platforms.Android.Workers;

public class TaskReminderWorker : Worker
{
    public TaskReminderWorker(Context context, WorkerParameters workerParams)
        : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        var title = InputData.GetString("title") ?? "Tarea";
        var description = InputData.GetString("description") ?? "";
        var dueDateRaw = InputData.GetString("dueDate");
        var priority = InputData.GetInt("priority", 1);

        // Formateo inteligente
        string dueText = "";
        if (!string.IsNullOrWhiteSpace(dueDateRaw) &&
            DateTime.TryParse(dueDateRaw, out var dueDate))
        {
            var today = DateTime.Today;

            if (dueDate.Date == today)
                dueText = $"📅 Vence hoy {dueDate:HH:mm}";
            else if (dueDate.Date == today.AddDays(1))
                dueText = $"📅 Vence mañana {dueDate:HH:mm}";
            else
                dueText = $"📅 {dueDate:dd/MM HH:mm}";
        }

        string priorityText = priority switch
        {
            0 => "🟢 Baja",
            1 => "🟡 Media",
            2 => "🔴 Alta",
            _ => ""
        };

        var finalDescription =
            $"{(string.IsNullOrWhiteSpace(description) ? "" : "📌 " + description + "\n")}" +
            $"{dueText}\n" +
            $"⚡ Prioridad: {priorityText}\n\n" +
            $"👉 Toca para abrir";

        var request = new NotificationRequest
        {
            NotificationId = Math.Abs(title.GetHashCode()),
            Title = $"⏰ {title}",
            Description = finalDescription
        };

        LocalNotificationCenter.Current.Show(request);

        Console.WriteLine("🔥 NOTIFICACIÓN PRO LANZADA");

        return Result.InvokeSuccess();
    }
}