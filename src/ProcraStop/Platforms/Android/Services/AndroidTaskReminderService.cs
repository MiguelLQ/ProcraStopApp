using AndroidX.Work;
using Java.Util.Concurrent;
using ProcraStop.Application.Tasks.Abstractions;
using AndroidApp = Android.App.Application;
namespace ProcraStop.Platforms.Android.Services;

public class AndroidTaskReminderService : ITaskReminderService
{
    public Task ScheduleAsync(
        Guid taskId,
        string title,
        string? description,
        DateTime reminderDate,
        DateTime? dueDate,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var delay = reminderDate - now;

        Console.WriteLine($"🕒 NOW: {now}");
        Console.WriteLine($"⏰ REMINDER: {reminderDate}");
        Console.WriteLine($"⌛ DELAY: {delay.TotalSeconds}s");

        if (delay.TotalMilliseconds <= 0)
        {
            Console.WriteLine("❌ Delay inválido");
            return Task.CompletedTask;
        }

        // En Android, el contexto es necesario para programar el WorkManager.
        // Usamos el contexto actual de la actividad o el contexto de la aplicación.
        var context = Platform.CurrentActivity ?? AndroidApp.Context;

        var data = new Data.Builder()
            .PutString("title", title)
            .PutString("description", description ?? "")
            .PutString("dueDate",
                dueDate.HasValue ? dueDate.Value.ToString("o") : "")
            .PutInt("priority", 2) // luego lo conectas al enum
            .Build();

        // Creamos un OneTimeWorkRequest para programar la tarea de recordatorio.
        var request = (OneTimeWorkRequest)new OneTimeWorkRequest.Builder(typeof(Workers.TaskReminderWorker))
            .SetInitialDelay((long)delay.TotalMilliseconds, TimeUnit.Milliseconds)
            .SetInputData(data)
            .Build();

        WorkManager.GetInstance(context)
            .EnqueueUniqueWork(
                taskId.ToString(),
                ExistingWorkPolicy.Replace,
                request);

        Console.WriteLine("WorkManager programado");

        return Task.CompletedTask;
    }
    // Para cancelar una tarea programada, simplemente cancelamos el trabajo único con el ID de la tarea.
    public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var context = Platform.CurrentActivity ?? AndroidApp.Context;

        WorkManager.GetInstance(context)
            .CancelUniqueWork(taskId.ToString());

        return Task.CompletedTask;
    }
}