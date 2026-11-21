using Android.Content;
using AndroidX.Work;
using ProcraStop.Platforms.Android.Services;

namespace ProcraStop.Platforms.Android.Workers;

public sealed class ReminderReconciliationWorker : Worker
{
    public const string UniqueWorkName = "procrastop-reminder-reconciliation";

    public ReminderReconciliationWorker(Context context, WorkerParameters workerParams)
        : base(context, workerParams) { }

    public override Result DoWork()
    {
        try
        {
            AndroidTaskReminderService.ReArmAllAsync(recordAudit: true).GetAwaiter().GetResult();
            return Result.InvokeSuccess();
        }
        catch
        {
            return Result.InvokeRetry();
        }
    }
}
