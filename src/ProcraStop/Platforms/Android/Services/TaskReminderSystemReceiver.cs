using Android.App;
using Android.Content;

namespace ProcraStop.Platforms.Android.Services;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter([Intent.ActionBootCompleted, Intent.ActionTimeChanged,
    Intent.ActionTimezoneChanged, "android.app.action.SCHEDULE_EXACT_ALARM_PERMISSION_STATE_CHANGED"])]
public sealed class TaskReminderSystemReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        var pendingResult = GoAsync();
        _ = ReArmAndFinishAsync(pendingResult);
    }

    private static async Task ReArmAndFinishAsync(PendingResult pendingResult)
    {
        try { await AndroidTaskReminderService.ReArmAllAsync(recordAudit: true); }
        finally { pendingResult.Finish(); }
    }
}
