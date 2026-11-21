using Android.App;
using Android.Content;

namespace ProcraStop.Maui11.Platforms.Android.Services;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class TaskReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent is null || !Guid.TryParse(intent.GetStringExtra("taskId"), out var taskId)) return;
        var pending = GoAsync();
        _ = FinishAsync(taskId, intent.GetStringExtra("kind") ?? AndroidTaskReminderService.DueKind,
            intent.GetLongExtra("ticks", 0), pending);
    }

    private static async Task FinishAsync(Guid taskId, string kind, long ticks, PendingResult? pending)
    {
        try { await ReminderRuntime.GetRequired<AndroidTaskReminderService>().DeliverAsync(taskId, kind, ticks); }
        finally { pending?.Finish(); }
    }
}
