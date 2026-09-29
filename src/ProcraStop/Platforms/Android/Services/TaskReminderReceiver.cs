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
        AndroidTaskReminderService.Deliver(context, taskId, intent.GetLongExtra("ticks", 0));
    }
}
