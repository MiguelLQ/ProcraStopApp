using Android.App;
using Android.Content;

namespace ProcraStop.Platforms.Android.Services;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class TaskReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null ||
            !Guid.TryParse(intent.GetStringExtra("taskId"), out var taskId)) return;

        var kind = (AndroidTaskReminderService.ReminderKind)intent.GetIntExtra("kind", 0);
        var pendingResult = GoAsync();
        _ = DeliverAndFinishAsync(
            context,
            taskId,
            kind,
            intent.GetLongExtra("ticks", 0),
            pendingResult);
    }

    private static async Task DeliverAndFinishAsync(
        Context context,
        Guid taskId,
        AndroidTaskReminderService.ReminderKind kind,
        long ticks,
        PendingResult pendingResult)
    {
        try { await AndroidTaskReminderService.DeliverAsync(context, taskId, kind, ticks); }
        finally { pendingResult.Finish(); }
    }
}
