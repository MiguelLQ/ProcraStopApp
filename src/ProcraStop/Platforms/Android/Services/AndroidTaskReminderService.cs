using AndroidX.Work;
using Java.Util.Concurrent;
using Microsoft.Maui.ApplicationModel;
using ProcraStop.Modules.Tasks.Application.Abstractions;
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

        // 🔥 CONTEXTO CORRECTO (sin ambigüedad)
        var context = Platform.CurrentActivity ?? AndroidApp.Context;

        var data = new Data.Builder()
            .PutString("title", title)
            .PutString("description", description ?? "")
            .PutString("dueDate",
                dueDate.HasValue ? dueDate.Value.ToString("o") : "")
            .PutInt("priority", 2) // luego lo conectas al enum
            .Build();

        // 🔥 FIX PARA ANDROIDX 2.11.x
        var request = (OneTimeWorkRequest)new OneTimeWorkRequest.Builder(typeof(Workers.TaskReminderWorker))
            .SetInitialDelay((long)delay.TotalMilliseconds, TimeUnit.Milliseconds)
            .SetInputData(data)
            .Build();

        WorkManager.GetInstance(context)
            .EnqueueUniqueWork(
                taskId.ToString(),
                ExistingWorkPolicy.Replace,
                request);

        Console.WriteLine("🔥 WorkManager programado");

        return Task.CompletedTask;
    }

    public Task CancelAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var context = Platform.CurrentActivity ?? AndroidApp.Context;

        WorkManager.GetInstance(context)
            .CancelUniqueWork(taskId.ToString());

        return Task.CompletedTask;
    }
}