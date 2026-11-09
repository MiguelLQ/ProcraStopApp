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
        AndroidTaskReminderService.Deliver(context, taskId, kind, intent.GetLongExtra("ticks", 0));
    }
}
